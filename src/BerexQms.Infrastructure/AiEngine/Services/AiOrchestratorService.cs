using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Infrastructure.AiEngine.Configuration;
using BerexQms.Infrastructure.AiEngine.Providers;
using BerexQms.Infrastructure.AiEngine.Providers.Local;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BerexQms.Infrastructure.AiEngine.Services;

/// <summary>
/// AI Orchestrator — the central coordination point for all AI reasoning requests.
///
/// Flow:
/// 1. Determine task type
/// 2. Resolve provider routing chain (primary → fallback1 → fallback2)
/// 3. Retrieve relevant QMS context (via context service)
/// 4. Build prompts (via prompt template manager)
/// 5. Validate context size (provider-aware limits)
/// 6. Call primary provider
/// 7. Validate response
/// 8. Walk fallback chain on technical failure
/// 9. Record usage
/// 10. Return normalized result
///
/// The orchestrator NEVER allows AI to directly modify the database,
/// bypass permissions, or execute application commands.
/// </summary>
internal sealed class AiOrchestratorService : IAiOrchestrator
{
    private readonly IReadOnlyDictionary<string, IAiProvider> _providers;
    private readonly IAiContextService _contextService;
    private readonly IAiUsageService _usageService;
    private readonly AiPromptTemplateManager _promptManager;
    private readonly AiProviderOptions _options;
    private readonly ILogger<AiOrchestratorService> _logger;

    public AiOrchestratorService(
        IEnumerable<IAiProvider> providers,
        IAiContextService contextService,
        IAiUsageService usageService,
        AiPromptTemplateManager promptManager,
        IOptions<AiProviderOptions> options,
        ILogger<AiOrchestratorService> logger)
    {
        _providers = providers.ToDictionary(p => p.ProviderName, p => p);
        _contextService = contextService;
        _usageService = usageService;
        _promptManager = promptManager;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AiProviderResponseDto> ExecuteAsync(
        AiOrchestratorRequest request, CancellationToken ct)
    {
        // 1. Resolve ordered provider chain for this task type
        var providerChain = ResolveProviderChain(request.TaskType);

        if (providerChain.Count == 0)
        {
            _logger.LogWarning("No provider configured for task type {TaskType}", request.TaskType);
            return FailedResponse("NoProvider", "No AI provider configured for this task type.");
        }

        // 2. Retrieve relevant context
        var contextSnippets = await RetrieveContextAsync(
            request.Module, request.MaxContextDocuments, ct);

        // 3. Build prompts
        var (systemPrompt, userPrompt, outputSchema) = await _promptManager.GetPromptsAsync(
            request.TaskType, request.Module, request.Content, ct);

        // 4. Build provider request (context will be trimmed per provider)
        var baseProviderRequest = new AiProviderRequestDto
        {
            TaskType = request.TaskType,
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt,
            ContextDocuments = contextSnippets,
            OutputSchema = outputSchema,
        };

        // 5. Walk the provider chain
        AiProviderResponseDto? response = null;
        var wasFallback = false;
        string? fallbackFrom = null;
        var attemptedProviders = new List<string>();

        foreach (var providerName in providerChain)
        {
            if (attemptedProviders.Count >= _options.MaxFallbackChain)
                break;

            // Track fallback state before calling provider — any attempt
            // beyond the first is a fallback, regardless of success/failure
            if (attemptedProviders.Count > 0)
            {
                wasFallback = true;
                fallbackFrom ??= attemptedProviders[0];
            }

            // Apply provider-specific context size limit
            var providerRequest = ApplyContextLimit(baseProviderRequest, providerName);

            response = await CallProviderAsync(providerName, providerRequest, ct);
            attemptedProviders.Add(providerName);

            if (response.Success)
                break;

            if (!IsTechnicalFailure(response.ErrorCategory))
                break; // Business/auth failure — don't fallback

            _logger.LogWarning(
                "Provider {Provider} failed ({Error}) for task {TaskType}. {Remaining} fallback(s) remaining",
                providerName, response.ErrorCategory, request.TaskType,
                providerChain.Count - attemptedProviders.Count);
        }

        response ??= FailedResponse("NoProvider", "No AI provider available for this task type.");

        // 6. Record usage
        var contextIds = string.Join(",",
            contextSnippets.Select(c => c.DocumentId).Distinct());

        await _usageService.RecordUsageAsync(
            response,
            request.TaskType,
            request.UserId,
            null, // recommendationId — set later if recommendation is created
            contextIds,
            wasFallback,
            fallbackFrom,
            ct);

        return response;
    }

    public async Task<IReadOnlyList<AiProviderStatusDto>> GetProviderStatusAsync(
        CancellationToken ct)
    {
        var statuses = new List<AiProviderStatusDto>();

        foreach (var provider in _providers.Values)
        {
            if (provider is ClaudeAiProvider claude)
                statuses.Add(claude.GetStatus());
            else if (provider is OpenAiProvider openAi)
                statuses.Add(openAi.GetStatus());
            else if (provider is LocalAiProvider local)
            {
                // Run health check for Local provider to get fresh status
                await local.CheckHealthAsync(ct);
                statuses.Add(local.GetStatus());
            }
        }

        return statuses;
    }

    public IReadOnlyList<AiTaskMappingDto> GetTaskMappings()
    {
        // Use ProviderRouting if populated, otherwise fall back to legacy TaskMappings
        if (_options.ProviderRouting.Count > 0)
        {
            return _options.ProviderRouting.Select(kv =>
            {
                var chain = kv.Value;
                return new AiTaskMappingDto
                {
                    TaskType = kv.Key,
                    PrimaryProvider = chain.Count > 0 ? chain[0] : string.Empty,
                    FallbackProvider = chain.Count > 1 ? string.Join(" → ", chain.Skip(1)) : null,
                };
            }).ToList();
        }

        return _options.TaskMappings.Select(kv => new AiTaskMappingDto
        {
            TaskType = kv.Key,
            PrimaryProvider = kv.Value,
            FallbackProvider = _options.FallbackMappings.GetValueOrDefault(kv.Key),
        }).ToList();
    }

    public async Task<IReadOnlyList<AiLocalModelDto>> GetLocalModelsAsync(
        CancellationToken ct)
    {
        if (!_providers.TryGetValue("Local", out var provider) || !provider.IsEnabled)
            return [];

        if (provider is not LocalAiProvider localProvider)
            return [];

        var models = await localProvider.ListAvailableModelsAsync(ct);
        return models;
    }

    // ---- Private helpers ----

    /// <summary>
    /// Resolve the ordered provider chain for a task type.
    /// Uses ProviderRouting if available, otherwise falls back to legacy TaskMappings/FallbackMappings.
    /// Only returns providers that are registered and enabled.
    /// </summary>
    private List<string> ResolveProviderChain(string taskType)
    {
        // Prefer new ProviderRouting
        if (_options.ProviderRouting.TryGetValue(taskType, out var routing) && routing.Count > 0)
        {
            return routing
                .Where(name => _providers.TryGetValue(name, out var p) && p.IsEnabled)
                .ToList();
        }

        // Legacy fallback: TaskMappings + FallbackMappings (2-provider chain)
        var chain = new List<string>();

        if (_options.TaskMappings.TryGetValue(taskType, out var primary))
        {
            if (_providers.TryGetValue(primary, out var pp) && pp.IsEnabled)
                chain.Add(primary);
        }

        if (_options.FallbackMappings.TryGetValue(taskType, out var fallback))
        {
            if (fallback != primary && _providers.TryGetValue(fallback, out var fp) && fp.IsEnabled)
                chain.Add(fallback);
        }

        // If no configured provider, add any enabled provider
        if (chain.Count == 0)
        {
            var anyEnabled = _providers.Values.FirstOrDefault(p => p.IsEnabled);
            if (anyEnabled != null)
                chain.Add(anyEnabled.ProviderName);
        }

        return chain;
    }

    /// <summary>
    /// Apply provider-specific context size limits.
    /// Local provider has a smaller context window than cloud providers.
    /// </summary>
    private AiProviderRequestDto ApplyContextLimit(
        AiProviderRequestDto request, string providerName)
    {
        var maxChars = providerName == "Local"
            ? _options.MaxLocalContextSizeChars
            : _options.MaxContextSizeChars;

        var totalContextChars = request.ContextDocuments.Sum(c => c.Content.Length);

        if (totalContextChars <= maxChars)
            return request;

        var trimmed = TrimContext(request.ContextDocuments, maxChars);
        return request with { ContextDocuments = trimmed };
    }

    private async Task<IReadOnlyList<AiContextSnippetDto>> RetrieveContextAsync(
        string? module, int maxDocuments, CancellationToken ct)
    {
        try
        {
            var searchResults = await _contextService.SearchRelevantContextAsync(
                module ?? string.Empty, module, maxDocuments, ct);

            return searchResults.Select(r => new AiContextSnippetDto
            {
                DocumentId = r.DocumentId.ToString(),
                SourceModule = r.SourceModule,
                ContextType = r.ContextType,
                Title = r.Title,
                Content = r.ContentSnippet,
                RelevanceScore = r.RelevanceScore,
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve context for module {Module}", module);
            return [];
        }
    }

    private async Task<AiProviderResponseDto> CallProviderAsync(
        string providerName, AiProviderRequestDto request, CancellationToken ct)
    {
        if (!_providers.TryGetValue(providerName, out var provider))
            return FailedResponse("ProviderNotFound", $"Provider '{providerName}' not registered.");

        if (!provider.IsEnabled)
            return FailedResponse("ProviderDisabled", $"Provider '{providerName}' is disabled.");

        return request.TaskType switch
        {
            "DocumentAnalysis" or "AuditAnalysis" =>
                await provider.AnalyzeAsync(request, ct),
            "RecommendationGeneration" or "CAPAAnalysis" or "SupplierAnalysis" =>
                await provider.GenerateRecommendationAsync(request, ct),
            "Summarization" =>
                await provider.SummarizeAsync(request, ct),
            "RiskAnalysis" or "DefectTrendAnalysis" or "QualityAnalysis" =>
                await provider.AnalyzeAsync(request, ct),
            "StructuredDataExtraction" =>
                await provider.ExtractStructuredDataAsync(request, ct),
            _ => await provider.AnalyzeAsync(request, ct),
        };
    }

    private static bool IsTechnicalFailure(string? errorCategory)
    {
        // Only failover for technical failures, NOT for auth/business errors
        return errorCategory is "Timeout" or "NetworkError" or "RateLimit"
            or "MaxRetriesExceeded" or "HttpError" or "EmptyResponse"
            or "ValidationError" or "ProviderError";
    }

    private static IReadOnlyList<AiContextSnippetDto> TrimContext(
        IReadOnlyList<AiContextSnippetDto> snippets, int maxChars)
    {
        var result = new List<AiContextSnippetDto>();
        var totalChars = 0;

        foreach (var snippet in snippets.OrderByDescending(s => s.RelevanceScore))
        {
            if (totalChars + snippet.Content.Length > maxChars)
                break;

            result.Add(snippet);
            totalChars += snippet.Content.Length;
        }

        return result;
    }

    private static AiProviderResponseDto FailedResponse(string category, string message) =>
        new()
        {
            Success = false,
            ErrorCategory = category,
            ErrorMessage = message,
            RequiresHumanReview = true,
            TokenUsage = new AiTokenUsageDto(),
        };
}
