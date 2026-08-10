using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Infrastructure.AiEngine.Configuration;
using BerexQms.Infrastructure.AiEngine.Providers.Local;
using BerexQms.Infrastructure.AiEngine.Providers.Local.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BerexQms.Infrastructure.Tests.AiEngine.Providers.Local;

public class LocalAiProviderTests
{
    private readonly OllamaClient _client;
    private readonly IOptions<AiProviderOptions> _options;
    private readonly ILogger<LocalAiProvider> _logger;
    private readonly LocalAiProvider _provider;

    private static readonly AiProviderOptions DefaultOptions = new()
    {
        Local = new LocalProviderOptions
        {
            Enabled = true,
            DefaultModel = "qwen3",
            TimeoutSeconds = 120,
            MaxRetries = 1,
            MaxTokens = 4096,
            Temperature = 0.1m,
        },
    };

    public LocalAiProviderTests()
    {
        _options = Options.Create(DefaultOptions);
        _client = Substitute.For<OllamaClient>(
            new HttpClient(),
            _options,
            NullLogger<OllamaClient>.Instance);
        _logger = NullLogger<LocalAiProvider>.Instance;
        _provider = new LocalAiProvider(_client, _options, _logger);
    }

    // ---- Basic provider contract ----

    [Fact]
    public void ProviderName_ReturnsLocal()
    {
        Assert.Equal("Local", _provider.ProviderName);
    }

    [Fact]
    public void IsEnabled_WhenConfigured_ReturnsTrue()
    {
        Assert.True(_provider.IsEnabled);
    }

    [Fact]
    public void IsEnabled_WhenDisabled_ReturnsFalse()
    {
        var opts = Options.Create(new AiProviderOptions
        {
            Local = new LocalProviderOptions { Enabled = false },
        });
        var provider = new LocalAiProvider(_client, opts, _logger);

        Assert.False(provider.IsEnabled);
    }

    // ---- Successful request ----

    [Fact]
    public async Task AnalyzeAsync_SuccessfulResponse_ReturnsSuccessResult()
    {
        var request = CreateRequest("DocumentAnalysis");
        SetupSuccessfulResponse("Analysis result text", inputTokens: 100, outputTokens: 50);

        var result = await _provider.AnalyzeAsync(request, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Local", result.Provider);
        Assert.Equal("Analysis result text", result.Content);
        Assert.True(result.RequiresHumanReview);
    }

    [Fact]
    public async Task GenerateRecommendationAsync_ReturnsSuccessResult()
    {
        var request = CreateRequest("RecommendationGeneration");
        SetupSuccessfulResponse("Recommendation content");

        var result = await _provider.GenerateRecommendationAsync(request, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Recommendation content", result.Content);
    }

    [Fact]
    public async Task SummarizeAsync_ReturnsSuccessResult()
    {
        var request = CreateRequest("Summarization");
        SetupSuccessfulResponse("Summary content");

        var result = await _provider.SummarizeAsync(request, CancellationToken.None);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task ExplainAsync_ReturnsSuccessResult()
    {
        var request = CreateRequest("RiskAnalysis");
        SetupSuccessfulResponse("Explanation content");

        var result = await _provider.ExplainAsync(request, CancellationToken.None);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task ExtractStructuredDataAsync_ReturnsSuccessResult()
    {
        var request = CreateRequest("StructuredDataExtraction");
        SetupSuccessfulResponse("Extracted data");

        var result = await _provider.ExtractStructuredDataAsync(request, CancellationToken.None);

        Assert.True(result.Success);
    }

    // ---- Token usage and cost ----

    [Fact]
    public async Task AnalyzeAsync_TokenValues_RecordedCorrectly()
    {
        var request = CreateRequest("DocumentAnalysis");
        SetupSuccessfulResponse("Result", inputTokens: 250, outputTokens: 100);

        var result = await _provider.AnalyzeAsync(request, CancellationToken.None);

        Assert.Equal(250, result.TokenUsage.InputTokens);
        Assert.Equal(100, result.TokenUsage.OutputTokens);
        Assert.Equal(350, result.TokenUsage.TotalTokens);
    }

    [Fact]
    public async Task AnalyzeAsync_CostAlwaysZero_ForLocalProvider()
    {
        var request = CreateRequest("DocumentAnalysis");
        SetupSuccessfulResponse("Result", inputTokens: 5000, outputTokens: 2000);

        var result = await _provider.AnalyzeAsync(request, CancellationToken.None);

        Assert.Equal(0m, result.TokenUsage.EstimatedCostUsd);
    }

    [Fact]
    public async Task AnalyzeAsync_NullableTokenValues_DefaultToZero()
    {
        var request = CreateRequest("DocumentAnalysis");
        _client.ResolveModel(Arg.Any<string>()).Returns("qwen3");
        _client.ChatAsync(Arg.Any<OllamaChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(new OllamaChatResponse
            {
                Model = "qwen3",
                Done = true,
                Message = new OllamaChatMessage { Role = "assistant", Content = "Result" },
                // No token counts
                PromptEvalCount = null,
                EvalCount = null,
            });

        var result = await _provider.AnalyzeAsync(request, CancellationToken.None);

        Assert.Equal(0, result.TokenUsage.InputTokens);
        Assert.Equal(0, result.TokenUsage.OutputTokens);
        Assert.Equal(0, result.TokenUsage.TotalTokens);
    }

    // ---- Error handling ----

    [Fact]
    public async Task AnalyzeAsync_NullResponse_ReturnsNetworkError()
    {
        var request = CreateRequest("DocumentAnalysis");
        _client.ResolveModel(Arg.Any<string>()).Returns("qwen3");
        _client.ChatAsync(Arg.Any<OllamaChatRequest>(), Arg.Any<CancellationToken>())
            .Returns((OllamaChatResponse?)null);

        var result = await _provider.AnalyzeAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("NetworkError", result.ErrorCategory);
    }

    [Fact]
    public async Task AnalyzeAsync_ErrorInResponse_ReturnsProviderError()
    {
        var request = CreateRequest("DocumentAnalysis");
        _client.ResolveModel(Arg.Any<string>()).Returns("qwen3");
        _client.ChatAsync(Arg.Any<OllamaChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(new OllamaChatResponse
            {
                Done = true,
                Error = "model not found",
            });

        var result = await _provider.AnalyzeAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("ProviderError", result.ErrorCategory);
        Assert.Contains("model not found", result.ErrorMessage);
    }

    [Fact]
    public async Task AnalyzeAsync_EmptyResponse_ReturnsEmptyResponseError()
    {
        var request = CreateRequest("DocumentAnalysis");
        _client.ResolveModel(Arg.Any<string>()).Returns("qwen3");
        _client.ChatAsync(Arg.Any<OllamaChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(new OllamaChatResponse
            {
                Model = "qwen3",
                Done = true,
                Message = new OllamaChatMessage { Role = "assistant", Content = "" },
            });

        var result = await _provider.AnalyzeAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("EmptyResponse", result.ErrorCategory);
    }

    [Fact]
    public async Task AnalyzeAsync_Timeout_ReturnsTimeoutError()
    {
        var request = CreateRequest("DocumentAnalysis");
        _client.ResolveModel(Arg.Any<string>()).Returns("qwen3");
        _client.ChatAsync(Arg.Any<OllamaChatRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Timeout", null, CancellationToken.None));

        var result = await _provider.AnalyzeAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Timeout", result.ErrorCategory);
    }

    [Fact]
    public async Task AnalyzeAsync_HttpError_ReturnsNetworkError()
    {
        var request = CreateRequest("DocumentAnalysis");
        _client.ResolveModel(Arg.Any<string>()).Returns("qwen3");
        _client.ChatAsync(Arg.Any<OllamaChatRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var result = await _provider.AnalyzeAsync(request, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("NetworkError", result.ErrorCategory);
    }

    // ---- Structured output validation ----

    [Fact]
    public async Task AnalyzeAsync_ValidJson_WithOutputSchema_Succeeds()
    {
        var request = CreateRequest("StructuredDataExtraction") with
        {
            OutputSchema = """{"type":"object","properties":{"items":{"type":"array"}}}""",
        };
        _client.ResolveModel(Arg.Any<string>()).Returns("qwen3");
        _client.ChatAsync(Arg.Any<OllamaChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(new OllamaChatResponse
            {
                Model = "qwen3",
                Done = true,
                Message = new OllamaChatMessage
                {
                    Role = "assistant",
                    Content = """{"items":["defect-A","defect-B"]}""",
                },
                PromptEvalCount = 100,
                EvalCount = 50,
            });

        var result = await _provider.AnalyzeAsync(request, CancellationToken.None);

        Assert.True(result.Success);
    }

    [Fact]
    public async Task AnalyzeAsync_InvalidJson_WithOutputSchema_TriesCorrectionPrompt()
    {
        var request = CreateRequest("StructuredDataExtraction") with
        {
            OutputSchema = """{"type":"object"}""",
        };
        _client.ResolveModel(Arg.Any<string>()).Returns("qwen3");

        // First call returns invalid JSON, second returns valid
        _client.ChatAsync(Arg.Any<OllamaChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(
                new OllamaChatResponse
                {
                    Model = "qwen3",
                    Done = true,
                    Message = new OllamaChatMessage { Role = "assistant", Content = "not valid json at all" },
                    PromptEvalCount = 100,
                    EvalCount = 50,
                },
                new OllamaChatResponse
                {
                    Model = "qwen3",
                    Done = true,
                    Message = new OllamaChatMessage { Role = "assistant", Content = """{"result":"ok"}""" },
                    PromptEvalCount = 120,
                    EvalCount = 40,
                });

        var result = await _provider.AnalyzeAsync(request, CancellationToken.None);

        Assert.True(result.Success);
    }

    // ---- Health checks ----

    [Fact]
    public async Task CheckHealthAsync_ServerReachableModelAvailable_ReturnsHealthy()
    {
        _client.IsServerReachableAsync(Arg.Any<CancellationToken>()).Returns(true);
        _client.IsModelAvailableAsync("qwen3", Arg.Any<CancellationToken>()).Returns(true);

        var status = await _provider.CheckHealthAsync(CancellationToken.None);

        Assert.Equal(LocalHealthStatus.Healthy, status);
    }

    [Fact]
    public async Task CheckHealthAsync_ServerUnreachable_ReturnsServerUnavailable()
    {
        _client.IsServerReachableAsync(Arg.Any<CancellationToken>()).Returns(false);

        var status = await _provider.CheckHealthAsync(CancellationToken.None);

        Assert.Equal(LocalHealthStatus.ServerUnavailable, status);
    }

    [Fact]
    public async Task CheckHealthAsync_ModelMissing_ReturnsModelMissing()
    {
        _client.IsServerReachableAsync(Arg.Any<CancellationToken>()).Returns(true);
        _client.IsModelAvailableAsync("qwen3", Arg.Any<CancellationToken>()).Returns(false);

        var status = await _provider.CheckHealthAsync(CancellationToken.None);

        Assert.Equal(LocalHealthStatus.ModelMissing, status);
    }

    // ---- GetStatus ----

    [Fact]
    public void GetStatus_ReturnsCorrectProviderInfo()
    {
        var status = _provider.GetStatus();

        Assert.Equal("Local", status.Provider);
        Assert.True(status.IsEnabled);
        Assert.Equal("qwen3", status.Model);
        Assert.Equal(120, status.TimeoutSeconds);
        Assert.Equal(1, status.MaxRetries);
        Assert.False(status.HasApiCost);
    }

    [Fact]
    public void GetStatus_IncludesSupportedTaskTypes()
    {
        var status = _provider.GetStatus();

        Assert.Contains("DocumentAnalysis", status.SupportedTaskTypes);
        Assert.Contains("QualityAnalysis", status.SupportedTaskTypes);
        Assert.Contains("Summarization", status.SupportedTaskTypes);
        Assert.Contains("RiskAnalysis", status.SupportedTaskTypes);
        Assert.Contains("StructuredDataExtraction", status.SupportedTaskTypes);
    }

    [Fact]
    public async Task GetStatus_AfterSuccessfulRequest_IsHealthy()
    {
        var request = CreateRequest("DocumentAnalysis");
        SetupSuccessfulResponse("Result");
        await _provider.AnalyzeAsync(request, CancellationToken.None);

        // Also set model as available for full health
        _client.IsServerReachableAsync(Arg.Any<CancellationToken>()).Returns(true);
        _client.IsModelAvailableAsync("qwen3", Arg.Any<CancellationToken>()).Returns(true);
        await _provider.CheckHealthAsync(CancellationToken.None);

        var status = _provider.GetStatus();
        Assert.True(status.IsHealthy);
        Assert.NotNull(status.LastSuccessAt);
    }

    // ---- Model resolution ----

    [Fact]
    public async Task AnalyzeAsync_UsesResolvedModel()
    {
        _client.ResolveModel("DocumentAnalysis").Returns("qwen3-docs");
        _client.ChatAsync(Arg.Any<OllamaChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(new OllamaChatResponse
            {
                Model = "qwen3-docs",
                Done = true,
                Message = new OllamaChatMessage { Role = "assistant", Content = "Result" },
                PromptEvalCount = 50,
                EvalCount = 25,
            });

        var request = CreateRequest("DocumentAnalysis");
        var result = await _provider.AnalyzeAsync(request, CancellationToken.None);

        Assert.Equal("qwen3-docs", result.Model);
    }

    // ---- List available models ----

    [Fact]
    public async Task ListAvailableModelsAsync_ReturnsModelList()
    {
        _client.ListModelsAsync(Arg.Any<CancellationToken>())
            .Returns(new OllamaTagsResponse
            {
                Models =
                [
                    new OllamaModelInfo { Name = "qwen3:latest", Size = 4_000_000_000 },
                    new OllamaModelInfo { Name = "llama3.1:8b", Size = 8_000_000_000 },
                ],
            });

        var models = await _provider.ListAvailableModelsAsync(CancellationToken.None);

        Assert.Equal(2, models.Count);
        Assert.Equal("qwen3:latest", models[0].Name);
        Assert.True(models[0].Available);
        Assert.Equal("llama3.1:8b", models[1].Name);
    }

    [Fact]
    public async Task ListAvailableModelsAsync_ServerUnavailable_ReturnsEmpty()
    {
        _client.ListModelsAsync(Arg.Any<CancellationToken>())
            .Returns((OllamaTagsResponse?)null);

        var models = await _provider.ListAvailableModelsAsync(CancellationToken.None);

        Assert.Empty(models);
    }

    // ---- Cancellation ----

    [Fact]
    public async Task AnalyzeAsync_Cancellation_ThrowsOperationCanceled()
    {
        var request = CreateRequest("DocumentAnalysis");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _client.ResolveModel(Arg.Any<string>()).Returns("qwen3");
        _client.ChatAsync(Arg.Any<OllamaChatRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("cancelled", null, cts.Token));

        await Assert.ThrowsAsync<TaskCanceledException>(
            () => _provider.AnalyzeAsync(request, cts.Token));
    }

    // ---- Helpers ----

    private static AiProviderRequestDto CreateRequest(string taskType) => new()
    {
        TaskType = taskType,
        SystemPrompt = "You are a QMS analyst.",
        UserPrompt = "Analyze this data.",
        ContextDocuments = [],
    };

    private void SetupSuccessfulResponse(
        string content, int inputTokens = 100, int outputTokens = 50)
    {
        _client.ResolveModel(Arg.Any<string>()).Returns("qwen3");
        _client.ChatAsync(Arg.Any<OllamaChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(new OllamaChatResponse
            {
                Model = "qwen3",
                Done = true,
                Message = new OllamaChatMessage { Role = "assistant", Content = content },
                PromptEvalCount = inputTokens,
                EvalCount = outputTokens,
            });
    }
}
