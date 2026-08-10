using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Infrastructure.AiEngine.Configuration;
using BerexQms.Infrastructure.AiEngine.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BerexQms.Infrastructure.AiEngine.Services;

/// <summary>
/// AI Orchestrator — the central coordination point for all AI reasoning requests.
///
/// Flow:
/// 1. Determine task type
/// 2. Select provider based on configuration
/// 3. Retrieve relevant QMS context (via context service)
/// 4. Build prompts (via prompt template manager)
/// 5. Validate context size
/// 6. Call AI provider
/// 7. Validate response
/// 8. Attempt failover if primary fails
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
        // 1. Resolve provider for this task type
        var primaryProviderName = ResolveProvider(request.TaskType);
        var fallbackProviderName = ResolveFallbackProvider(request.TaskType);

        if (primaryProviderName == null)
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

        // 4. Validate context size
        var totalContextChars = contextSnippets.Sum(c => c.Content.Length);
        if (totalContextChars > _options.MaxContextSizeChars)
        {
            // Trim to fit — take highest relevance first
            contextSnippets = TrimContext(contextSnippets, _options.MaxContextSizeChars);
        }

        // 5. Build provider request
        var providerRequest = new AiProviderRequestDto
        {
            TaskType = request.TaskType,
            SystemPrompt = systemPrompt,
            UserPrompt = userPrompt,
            ContextDocuments = contextSnippets,
            OutputSchema = outputSchema,
        };

        // 6. Call primary provider
        var response = await CallProviderAsync(primaryProviderName, providerRequest, ct);

        // 7. Failover if primary failed
        var wasFallback = false;
        string? fallbackFrom = null;

        if (!response.Success && fallbackProviderName != null
            && fallbackProviderName != primaryProviderName
            && IsTechnicalFailure(response.ErrorCategory))
        {
            _logger.LogWarning(
                "Primary provider {Primary} failed ({Error}). Falling back to {Fallback}",
                primaryProviderName, response.ErrorCategory, fallbackProviderName);

            fallbackFrom = primaryProviderName;
            response = await CallProviderAsync(fallbackProviderName, providerRequest, ct);
            wasFallback = true;
        }

        // 8. Record usage
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
        await Task.CompletedTask;

        var statuses = new List<AiProviderStatusDto>();

        foreach (var provider in _providers.Values)
        {
            if (provider is ClaudeAiProvider claude)
                statuses.Add(claude.GetStatus());
            else if (provider is OpenAiProvider openAi)
                statuses.Add(openAi.GetStatus());
        }

        return statuses;
    }

    public IReadOnlyList<AiTaskMappingDto> GetTaskMappings()
    {
        return _options.TaskMappings.Select(kv => new AiTaskMappingDto
        {
            TaskType = kv.Key,
            PrimaryProvider = kv.Value,
            FallbackProvider = _options.FallbackMappings.GetValueOrDefault(kv.Key),
        }).ToList();
    }

    // ---- Private helpers ----

    private string? ResolveProvider(string taskType)
    {
        if (_options.TaskMappings.TryGetValue(taskType, out var provider))
        {
            // If configured provider is disabled, try fallback
            if (_providers.TryGetValue(provider, out var p) && p.IsEnabled)
                return provider;

            // Primary disabled, try fallback
            var fallback = ResolveFallbackProvider(taskType);
            if (fallback != null && _providers.TryGetValue(fallback, out var fb) && fb.IsEnabled)
                return fallback;
        }

        // No mapping — pick first enabled provider
        return _providers.Values.FirstOrDefault(p => p.IsEnabled)?.ProviderName;
    }

    private string? ResolveFallbackProvider(string taskType)
    {
        if (_options.FallbackMappings.TryGetValue(taskType, out var fallback))
            return fallback;
        return null;
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
            or "MaxRetriesExceeded" or "HttpError";
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
