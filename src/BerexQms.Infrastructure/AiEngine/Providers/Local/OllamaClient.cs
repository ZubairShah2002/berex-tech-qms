using System.Text;
using System.Text.Json;
using BerexQms.Infrastructure.AiEngine.Configuration;
using BerexQms.Infrastructure.AiEngine.Providers.Local.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BerexQms.Infrastructure.AiEngine.Providers.Local;

/// <summary>
/// Typed HTTP client for Ollama API communication.
/// Uses HttpClientFactory for lifecycle management and supports
/// cancellation tokens, configurable timeouts, and safe error handling.
///
/// SECURITY: Never logs prompt/response content. Logs only structural
/// metadata (model name, status codes, timing).
/// </summary>
internal class OllamaClient
{
    private readonly HttpClient _httpClient;
    private readonly LocalProviderOptions _options;
    private readonly ILogger<OllamaClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };

    public OllamaClient(
        HttpClient httpClient,
        IOptions<AiProviderOptions> options,
        ILogger<OllamaClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value.Local;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    /// <summary>
    /// Send a chat completion request to Ollama.
    /// Returns null on infrastructure failure (HTTP error, timeout, deserialization failure).
    /// </summary>
    public virtual async Task<OllamaChatResponse?> ChatAsync(
        OllamaChatRequest request, CancellationToken ct)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/chat", content, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Ollama chat request failed with HTTP {StatusCode} for model {Model}",
                    (int)response.StatusCode, request.Model);

                var errorBody = await response.Content.ReadAsStringAsync(ct);
                return new OllamaChatResponse
                {
                    Done = true,
                    Error = $"HTTP {(int)response.StatusCode}",
                };
            }

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<OllamaChatResponse>(responseJson, JsonOptions);

            return result;
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("Ollama chat request timed out for model {Model}", request.Model);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Ollama network error for model {Model}", request.Model);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize Ollama response for model {Model}", request.Model);
            return null;
        }
    }

    /// <summary>
    /// Check whether the Ollama server is reachable.
    /// Uses a short timeout independent of the request timeout.
    /// </summary>
    public virtual async Task<bool> IsServerReachableAsync(CancellationToken ct)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.HealthCheckTimeoutSeconds));

            // Ollama returns "Ollama is running" at the root endpoint
            var response = await _httpClient.GetAsync("/", cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// List installed models. Returns null if the server is unreachable.
    /// </summary>
    public virtual async Task<OllamaTagsResponse?> ListModelsAsync(CancellationToken ct)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.HealthCheckTimeoutSeconds));

            var response = await _httpClient.GetAsync("/api/tags", cts.Token);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cts.Token);
            return JsonSerializer.Deserialize<OllamaTagsResponse>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Check whether a specific model is available on the Ollama server.
    /// </summary>
    public virtual async Task<bool> IsModelAvailableAsync(string modelName, CancellationToken ct)
    {
        var tags = await ListModelsAsync(ct);
        if (tags?.Models == null)
            return false;

        // Ollama model names may include tags like ":latest"
        return tags.Models.Any(m =>
            m.Name.Equals(modelName, StringComparison.OrdinalIgnoreCase)
            || m.Name.StartsWith(modelName + ":", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Resolve the model name for a given task type.
    /// Uses per-task overrides from configuration, falling back to the default model.
    /// </summary>
    public virtual string ResolveModel(string? taskType)
    {
        if (taskType != null && _options.TaskModels.TryGetValue(taskType, out var taskModel))
            return taskModel;

        return _options.DefaultModel;
    }
}
