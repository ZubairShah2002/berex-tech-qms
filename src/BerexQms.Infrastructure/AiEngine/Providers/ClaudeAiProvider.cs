using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Infrastructure.AiEngine.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BerexQms.Infrastructure.AiEngine.Providers;

/// <summary>
/// Claude (Anthropic) AI provider implementation.
/// Handles structured prompts, context injection, API communication,
/// timeouts, transient failures, and rate limits.
///
/// Claude is primarily used for:
/// - Long-form QMS document reasoning
/// - SOP / ISO / QMS document analysis
/// - Supplier corrective action analysis
/// - Audit report analysis
/// - CAPA analysis
/// - Complex quality reasoning
/// - Cross-document reasoning
///
/// Claude NEVER directly executes application actions.
/// </summary>
internal sealed class ClaudeAiProvider : IAiProvider
{
    private readonly HttpClient _httpClient;
    private readonly ClaudeProviderOptions _options;
    private readonly ILogger<ClaudeAiProvider> _logger;
    private DateTime? _lastSuccessAt;
    private DateTime? _lastErrorAt;
    private string? _lastErrorMessage;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public ClaudeAiProvider(
        HttpClient httpClient,
        IOptions<AiProviderOptions> options,
        ILogger<ClaudeAiProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value.Claude;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public string ProviderName => "Claude";
    public bool IsEnabled => _options.Enabled && !string.IsNullOrWhiteSpace(_options.ApiKey);

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
        IsHealthy = IsEnabled && (_lastErrorAt == null || _lastSuccessAt > _lastErrorAt),
        Model = _options.Model,
        TimeoutSeconds = _options.TimeoutSeconds,
        MaxRetries = _options.MaxRetries,
        SupportedTaskTypes = ["DocumentAnalysis", "CAPAAnalysis", "SupplierAnalysis", "AuditAnalysis"],
        LastErrorMessage = _lastErrorMessage,
        LastSuccessAt = _lastSuccessAt,
        LastErrorAt = _lastErrorAt,
    };

    private async Task<AiProviderResponseDto> SendRequestAsync(
        AiProviderRequestDto request, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var retryCount = 0;

        while (retryCount <= _options.MaxRetries)
        {
            try
            {
                var userContent = BuildUserContent(request);

                var body = new
                {
                    model = _options.Model,
                    max_tokens = request.MaxTokens ?? _options.MaxTokens,
                    system = request.SystemPrompt,
                    messages = new[]
                    {
                        new { role = "user", content = userContent }
                    },
                };

                var json = JsonSerializer.Serialize(body, JsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/v1/messages", content, ct);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    retryCount++;
                    if (retryCount > _options.MaxRetries)
                        return ErrorResponse(sw, "RateLimit", "Rate limited by Claude API.");

                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                    _logger.LogWarning("Claude rate limited. Retry {Retry}/{Max} in {Delay}s",
                        retryCount, _options.MaxRetries, delay.TotalSeconds);
                    await Task.Delay(delay, ct);
                    continue;
                }

                var responseJson = await response.Content.ReadAsStringAsync(ct);

                if (!response.IsSuccessStatusCode)
                {
                    _lastErrorAt = DateTime.UtcNow;
                    _lastErrorMessage = $"HTTP {(int)response.StatusCode}";
                    return ErrorResponse(sw, "HttpError",
                        $"Claude API returned {(int)response.StatusCode}.");
                }

                var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(
                    responseJson, JsonOptions);

                if (claudeResponse == null)
                    return ErrorResponse(sw, "ParseError", "Failed to parse Claude response.");

                _lastSuccessAt = DateTime.UtcNow;
                _lastErrorMessage = null;

                var outputText = claudeResponse.Content?
                    .FirstOrDefault(c => c.Type == "text")?.Text ?? string.Empty;

                var inputTokens = claudeResponse.Usage?.InputTokens ?? 0;
                var outputTokens = claudeResponse.Usage?.OutputTokens ?? 0;

                var recommendations = ParseStructuredRecommendations(outputText);

                sw.Stop();

                return new AiProviderResponseDto
                {
                    Success = true,
                    Provider = ProviderName,
                    Model = claudeResponse.Model ?? _options.Model,
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
                        InputTokens = inputTokens,
                        OutputTokens = outputTokens,
                        TotalTokens = inputTokens + outputTokens,
                        EstimatedCostUsd = (inputTokens / 1000m * _options.InputTokenCostPer1K)
                            + (outputTokens / 1000m * _options.OutputTokenCostPer1K),
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
                    return ErrorResponse(sw, "Timeout", "Claude API request timed out.");
                }

                var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                _logger.LogWarning("Claude timeout. Retry {Retry}/{Max}", retryCount, _options.MaxRetries);
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
                _logger.LogWarning(ex, "Claude network error. Retry {Retry}/{Max}", retryCount, _options.MaxRetries);
                await Task.Delay(delay, ct);
            }
        }

        return ErrorResponse(sw, "MaxRetriesExceeded", "Max retries exceeded for Claude API.");
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

    private static List<AiRecommendationItemDto> ParseStructuredRecommendations(string output)
    {
        var recommendations = new List<AiRecommendationItemDto>();

        try
        {
            // Try to extract JSON array from the response
            var jsonStart = output.IndexOf('[');
            var jsonEnd = output.LastIndexOf(']');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonPart = output[jsonStart..(jsonEnd + 1)];
                var items = JsonSerializer.Deserialize<List<AiRecommendationItemDto>>(
                    jsonPart, JsonOptions);
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
            Model = _options.Model,
            ErrorCategory = category,
            ErrorMessage = message,
            ProcessingTimeMs = sw.ElapsedMilliseconds,
            RequiresHumanReview = true,
            TokenUsage = new AiTokenUsageDto(),
        };
    }

    // ---- Claude API response models ----

    private sealed class ClaudeResponse
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public string? Role { get; set; }
        public string? Model { get; set; }
        public List<ClaudeContentBlock>? Content { get; set; }
        public ClaudeUsage? Usage { get; set; }
    }

    private sealed class ClaudeContentBlock
    {
        public string Type { get; set; } = string.Empty;
        public string? Text { get; set; }
    }

    private sealed class ClaudeUsage
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }
        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }
    }
}
