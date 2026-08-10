using System.Diagnostics;
using System.Text;
using System.Text.Json;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Infrastructure.AiEngine.Configuration;
using BerexQms.Infrastructure.AiEngine.Providers.Local.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BerexQms.Infrastructure.AiEngine.Providers.Local;

/// <summary>
/// Local AI provider implementation backed by Ollama-compatible inference.
///
/// Handles structured prompts, context injection, API communication,
/// timeouts, transient failures, and health checks against a local
/// Ollama server.
///
/// Local AI is suitable for:
/// - Document analysis and summarization
/// - Quality data analysis and trend detection
/// - Risk assessment
/// - Defect trend analysis
/// - Supplier analysis
/// - Structured data extraction
///
/// The provider API cost is always zero — infrastructure costs (electricity,
/// hardware, maintenance) are the operator's responsibility and not tracked here.
///
/// Local AI NEVER directly executes application actions.
/// </summary>
internal sealed class LocalAiProvider : IAiProvider
{
    private readonly OllamaClient _client;
    private readonly LocalProviderOptions _options;
    private readonly ILogger<LocalAiProvider> _logger;
    private DateTime? _lastSuccessAt;
    private DateTime? _lastErrorAt;
    private string? _lastErrorMessage;
    private bool? _lastModelAvailable;

    private static readonly JsonSerializerOptions ParseJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public LocalAiProvider(
        OllamaClient client,
        IOptions<AiProviderOptions> options,
        ILogger<LocalAiProvider> logger)
    {
        _client = client;
        _options = options.Value.Local;
        _logger = logger;
    }

    public string ProviderName => "Local";
    public bool IsEnabled => _options.Enabled;

    public Task<AiProviderResponseDto> AnalyzeAsync(
        AiProviderRequestDto request, CancellationToken ct) =>
        SendRequestAsync(request, ct);

    public Task<AiProviderResponseDto> GenerateRecommendationAsync(
        AiProviderRequestDto request, CancellationToken ct) =>
        SendRequestAsync(request, ct);

    public Task<AiProviderResponseDto> SummarizeAsync(
        AiProviderRequestDto request, CancellationToken ct) =>
        SendRequestAsync(request, ct);

    public Task<AiProviderResponseDto> ExplainAsync(
        AiProviderRequestDto request, CancellationToken ct) =>
        SendRequestAsync(request, ct);

    public Task<AiProviderResponseDto> ExtractStructuredDataAsync(
        AiProviderRequestDto request, CancellationToken ct) =>
        SendRequestAsync(request, ct);

    public AiProviderStatusDto GetStatus() => new()
    {
        Provider = ProviderName,
        IsEnabled = IsEnabled,
        IsHealthy = IsEnabled
            && (_lastErrorAt == null || _lastSuccessAt > _lastErrorAt)
            && (_lastModelAvailable ?? false),
        Model = _options.DefaultModel,
        TimeoutSeconds = _options.TimeoutSeconds,
        MaxRetries = _options.MaxRetries,
        SupportedTaskTypes = [
            "DocumentAnalysis", "QualityAnalysis", "Summarization",
            "RiskAnalysis", "SupplierAnalysis", "DefectTrendAnalysis",
            "StructuredDataExtraction", "AuditAnalysis",
        ],
        LastErrorMessage = _lastErrorMessage,
        LastSuccessAt = _lastSuccessAt,
        LastErrorAt = _lastErrorAt,
        HasApiCost = false,
    };

    /// <summary>
    /// List models available on the Ollama server, mapping each to an
    /// <see cref="AiLocalModelDto"/> with availability flag.
    /// Returns an empty list if the server is unreachable.
    /// </summary>
    public async Task<IReadOnlyList<AiLocalModelDto>> ListAvailableModelsAsync(CancellationToken ct)
    {
        var tags = await _client.ListModelsAsync(ct);
        if (tags?.Models == null)
            return [];

        return tags.Models.Select(m => new AiLocalModelDto
        {
            Name = m.Name,
            Available = true,
        }).ToList();
    }

    /// <summary>
    /// Check health: server reachable + default model available.
    /// </summary>
    public async Task<LocalHealthStatus> CheckHealthAsync(CancellationToken ct)
    {
        var serverReachable = await _client.IsServerReachableAsync(ct);
        if (!serverReachable)
        {
            _lastModelAvailable = false;
            return LocalHealthStatus.ServerUnavailable;
        }

        var modelAvailable = await _client.IsModelAvailableAsync(_options.DefaultModel, ct);
        _lastModelAvailable = modelAvailable;

        return modelAvailable ? LocalHealthStatus.Healthy : LocalHealthStatus.ModelMissing;
    }

    private async Task<AiProviderResponseDto> SendRequestAsync(
        AiProviderRequestDto request, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var model = _client.ResolveModel(request.TaskType);
        var retryCount = 0;

        while (retryCount <= _options.MaxRetries)
        {
            try
            {
                var userContent = BuildUserContent(request);

                var chatRequest = new OllamaChatRequest
                {
                    Model = model,
                    Stream = false,
                    Format = !string.IsNullOrWhiteSpace(request.OutputSchema) ? "json" : null,
                    Messages =
                    [
                        new OllamaChatMessage { Role = "system", Content = request.SystemPrompt },
                        new OllamaChatMessage { Role = "user", Content = userContent },
                    ],
                    Options = new OllamaRequestOptions
                    {
                        Temperature = (double)(request.Temperature ?? _options.Temperature),
                        NumPredict = request.MaxTokens ?? _options.MaxTokens,
                    },
                };

                var response = await _client.ChatAsync(chatRequest, ct);

                if (response == null)
                {
                    retryCount++;
                    if (retryCount > _options.MaxRetries)
                    {
                        _lastErrorAt = DateTime.UtcNow;
                        _lastErrorMessage = "No response from Ollama";
                        return ErrorResponse(sw, "NetworkError", "Local AI provider did not respond.");
                    }

                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                    _logger.LogWarning("Local AI no response. Retry {Retry}/{Max}", retryCount, _options.MaxRetries);
                    await Task.Delay(delay, ct);
                    continue;
                }

                if (!string.IsNullOrEmpty(response.Error))
                {
                    _lastErrorAt = DateTime.UtcNow;
                    _lastErrorMessage = response.Error;
                    return ErrorResponse(sw, "ProviderError", $"Local AI error: {response.Error}");
                }

                var outputText = response.Message?.Content ?? string.Empty;

                if (string.IsNullOrWhiteSpace(outputText))
                {
                    _lastErrorAt = DateTime.UtcNow;
                    _lastErrorMessage = "Empty response";
                    return ErrorResponse(sw, "EmptyResponse", "Local AI returned an empty response.");
                }

                // Validate structured output if schema was requested
                if (!string.IsNullOrWhiteSpace(request.OutputSchema))
                {
                    var validated = ValidateStructuredOutput(outputText);
                    if (!validated)
                    {
                        // Retry once with a correction prompt
                        if (retryCount == 0)
                        {
                            retryCount++;
                            _logger.LogWarning("Local AI returned invalid JSON. Retrying with correction prompt");

                            chatRequest.Messages.Add(new OllamaChatMessage
                            {
                                Role = "user",
                                Content = "Your previous response was not valid JSON. Please respond ONLY with valid JSON matching the requested format. No explanations or markdown.",
                            });

                            var correctionResponse = await _client.ChatAsync(chatRequest, ct);
                            if (correctionResponse?.Message?.Content != null
                                && ValidateStructuredOutput(correctionResponse.Message.Content))
                            {
                                outputText = correctionResponse.Message.Content;
                                response = correctionResponse;
                            }
                            else
                            {
                                _lastErrorAt = DateTime.UtcNow;
                                _lastErrorMessage = "Invalid structured output after retry";
                                return ErrorResponse(sw, "ValidationError",
                                    "Local AI failed to produce valid structured output after retry.");
                            }
                        }
                        else
                        {
                            _lastErrorAt = DateTime.UtcNow;
                            _lastErrorMessage = "Invalid structured output";
                            return ErrorResponse(sw, "ValidationError",
                                "Local AI returned invalid structured output.");
                        }
                    }
                }

                _lastSuccessAt = DateTime.UtcNow;
                _lastErrorMessage = null;

                // Ollama provides eval_count (output tokens) and prompt_eval_count (input tokens)
                // These may be null depending on the model and Ollama version
                var inputTokens = response.PromptEvalCount;
                var outputTokens = response.EvalCount;

                var recommendations = ParseStructuredRecommendations(outputText);

                sw.Stop();

                return new AiProviderResponseDto
                {
                    Success = true,
                    Provider = ProviderName,
                    Model = response.Model ?? model,
                    Content = outputText,
                    ConfidenceScore = recommendations.Count > 0
                        ? recommendations.Average(r => r.ConfidenceScore)
                        : 0.5m,
                    ReasoningSummary = outputText.Length > 500
                        ? outputText[..500] + "..."
                        : outputText,
                    Recommendations = recommendations,
                    TokenUsage = new AiTokenUsageDto
                    {
                        // Record actual token counts when available; zero when unavailable
                        InputTokens = inputTokens ?? 0,
                        OutputTokens = outputTokens ?? 0,
                        TotalTokens = (inputTokens ?? 0) + (outputTokens ?? 0),
                        // Local inference has zero API cost
                        EstimatedCostUsd = 0m,
                    },
                    ProcessingTimeMs = sw.ElapsedMilliseconds,
                    RequiresHumanReview = true,
                };
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested)
            {
                retryCount++;
                if (retryCount > _options.MaxRetries)
                {
                    _lastErrorAt = DateTime.UtcNow;
                    _lastErrorMessage = "Timeout";
                    return ErrorResponse(sw, "Timeout", "Local AI request timed out.");
                }

                var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                _logger.LogWarning("Local AI timeout. Retry {Retry}/{Max}", retryCount, _options.MaxRetries);
                await Task.Delay(delay, ct);
            }
            catch (HttpRequestException ex)
            {
                retryCount++;
                if (retryCount > _options.MaxRetries)
                {
                    _lastErrorAt = DateTime.UtcNow;
                    _lastErrorMessage = ex.Message;
                    return ErrorResponse(sw, "NetworkError", $"Network error: {ex.Message}");
                }

                var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                _logger.LogWarning(ex, "Local AI network error. Retry {Retry}/{Max}", retryCount, _options.MaxRetries);
                await Task.Delay(delay, ct);
            }
        }

        return ErrorResponse(sw, "MaxRetriesExceeded", "Max retries exceeded for Local AI provider.");
    }

    private static string BuildUserContent(AiProviderRequestDto request)
    {
        var sb = new StringBuilder();

        if (request.ContextDocuments.Count > 0)
        {
            sb.AppendLine("=== QMS CONTEXT ===");
            foreach (var doc in request.ContextDocuments)
            {
                sb.AppendLine($"[{doc.SourceModule} / {doc.ContextType}] {doc.Title}");
                sb.AppendLine(doc.Content);
                sb.AppendLine();
            }
            sb.AppendLine("=== END CONTEXT ===");
            sb.AppendLine();
        }

        sb.AppendLine(request.UserPrompt);

        if (!string.IsNullOrWhiteSpace(request.OutputSchema))
        {
            sb.AppendLine();
            sb.AppendLine("Respond in the following JSON format:");
            sb.AppendLine(request.OutputSchema);
        }

        return sb.ToString();
    }

    private static bool ValidateStructuredOutput(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return false;

        try
        {
            using var doc = JsonDocument.Parse(output);
            return true;
        }
        catch (JsonException)
        {
            // Try to extract JSON from potential markdown code blocks
            var trimmed = output.Trim();
            if (trimmed.StartsWith("```"))
            {
                var firstNewline = trimmed.IndexOf('\n');
                var lastFence = trimmed.LastIndexOf("```");
                if (firstNewline > 0 && lastFence > firstNewline)
                {
                    var jsonPart = trimmed[(firstNewline + 1)..lastFence].Trim();
                    try
                    {
                        using var innerDoc = JsonDocument.Parse(jsonPart);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }
    }

    private static List<AiRecommendationItemDto> ParseStructuredRecommendations(string output)
    {
        var recommendations = new List<AiRecommendationItemDto>();

        try
        {
            var jsonStart = output.IndexOf('[');
            var jsonEnd = output.LastIndexOf(']');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonPart = output[jsonStart..(jsonEnd + 1)];
                var items = JsonSerializer.Deserialize<List<AiRecommendationItemDto>>(
                    jsonPart, ParseJsonOptions);
                if (items != null)
                    recommendations.AddRange(items);
            }
        }
        catch
        {
            // Non-structured response — acceptable, recommendations remain empty
        }

        return recommendations;
    }

    private AiProviderResponseDto ErrorResponse(Stopwatch sw, string category, string message)
    {
        sw.Stop();
        return new AiProviderResponseDto
        {
            Success = false,
            Provider = ProviderName,
            Model = _options.DefaultModel,
            ErrorCategory = category,
            ErrorMessage = message,
            ProcessingTimeMs = sw.ElapsedMilliseconds,
            RequiresHumanReview = true,
            TokenUsage = new AiTokenUsageDto
            {
                EstimatedCostUsd = 0m,
            },
        };
    }
}

/// <summary>
/// Health status of the Local AI provider, distinguishing between
/// server availability and model availability.
/// </summary>
public enum LocalHealthStatus
{
    /// <summary>Server reachable and configured model available.</summary>
    Healthy,

    /// <summary>Server reachable but the configured model is not installed.</summary>
    ModelMissing,

    /// <summary>Server not reachable.</summary>
    ServerUnavailable,
}
