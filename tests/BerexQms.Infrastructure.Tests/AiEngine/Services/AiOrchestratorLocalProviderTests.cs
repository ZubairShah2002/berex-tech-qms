using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.AiEngine.Configuration;
using BerexQms.Infrastructure.AiEngine.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace BerexQms.Infrastructure.Tests.AiEngine.Services;

/// <summary>
/// Tests for multi-provider routing, fallback chains, and cost optimization
/// in the AI Orchestrator — Sprint 17 behavior.
/// </summary>
public class AiOrchestratorLocalProviderTests
{
    private readonly IAiContextService _contextService;
    private readonly IAiUsageService _usageService;
    private readonly AiPromptTemplateManager _promptManager;
    private readonly ILogger<AiOrchestratorService> _logger;

    public AiOrchestratorLocalProviderTests()
    {
        _contextService = Substitute.For<IAiContextService>();
        _usageService = Substitute.For<IAiUsageService>();
        _logger = NullLogger<AiOrchestratorService>.Instance;

        // Create real prompt manager with mocked repository
        var promptRepo = Substitute.For<IAiPromptTemplateRepository>();
        promptRepo.GetActiveByTaskTypeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Domain.AiEngine.Entities.AiPromptTemplate?)null);
        _promptManager = new AiPromptTemplateManager(promptRepo);

        // Default: context service returns empty list
        _contextService.SearchRelevantContextAsync(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<ContextSearchResultDto>());
    }

    [Fact]
    public async Task ExecuteAsync_LocalAsPrimary_UsesLocalProvider()
    {
        var localProvider = CreateMockProvider("Local", isEnabled: true, success: true);
        var claudeProvider = CreateMockProvider("Claude", isEnabled: true, success: true);

        var options = CreateOptions(new Dictionary<string, List<string>>
        {
            ["DocumentAnalysis"] = ["Local", "Claude"],
        });

        var orchestrator = CreateOrchestrator(options, localProvider, claudeProvider);

        var result = await orchestrator.ExecuteAsync(
            new AiOrchestratorRequest { TaskType = "DocumentAnalysis" }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Local", result.Provider);
    }

    [Fact]
    public async Task ExecuteAsync_LocalFails_FallsBackToClaude()
    {
        var localProvider = CreateMockProvider("Local", isEnabled: true, success: false, errorCategory: "Timeout");
        var claudeProvider = CreateMockProvider("Claude", isEnabled: true, success: true);

        var options = CreateOptions(new Dictionary<string, List<string>>
        {
            ["DocumentAnalysis"] = ["Local", "Claude"],
        });

        var orchestrator = CreateOrchestrator(options, localProvider, claudeProvider);

        var result = await orchestrator.ExecuteAsync(
            new AiOrchestratorRequest { TaskType = "DocumentAnalysis" }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Claude", result.Provider);
    }

    [Fact]
    public async Task ExecuteAsync_ClaudeFails_FallsBackToLocal()
    {
        var claudeProvider = CreateMockProvider("Claude", isEnabled: true, success: false, errorCategory: "NetworkError");
        var localProvider = CreateMockProvider("Local", isEnabled: true, success: true);

        var options = CreateOptions(new Dictionary<string, List<string>>
        {
            ["RecommendationGeneration"] = ["Claude", "Local"],
        });

        var orchestrator = CreateOrchestrator(options, claudeProvider, localProvider);

        var result = await orchestrator.ExecuteAsync(
            new AiOrchestratorRequest { TaskType = "RecommendationGeneration" }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Local", result.Provider);
    }

    [Fact]
    public async Task ExecuteAsync_LocalDisabled_SkipsLocal()
    {
        var localProvider = CreateMockProvider("Local", isEnabled: false, success: true);
        var claudeProvider = CreateMockProvider("Claude", isEnabled: true, success: true);

        var options = CreateOptions(new Dictionary<string, List<string>>
        {
            ["DocumentAnalysis"] = ["Local", "Claude"],
        });

        var orchestrator = CreateOrchestrator(options, localProvider, claudeProvider);

        var result = await orchestrator.ExecuteAsync(
            new AiOrchestratorRequest { TaskType = "DocumentAnalysis" }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("Claude", result.Provider);
    }

    [Fact]
    public async Task ExecuteAsync_AllProvidersFail_ReturnsError()
    {
        var localProvider = CreateMockProvider("Local", isEnabled: true, success: false, errorCategory: "Timeout");
        var claudeProvider = CreateMockProvider("Claude", isEnabled: true, success: false, errorCategory: "NetworkError");
        var openAiProvider = CreateMockProvider("OpenAi", isEnabled: true, success: false, errorCategory: "RateLimit");

        var options = CreateOptions(new Dictionary<string, List<string>>
        {
            ["DocumentAnalysis"] = ["Local", "Claude", "OpenAi"],
        });

        var orchestrator = CreateOrchestrator(options, localProvider, claudeProvider, openAiProvider);

        var result = await orchestrator.ExecuteAsync(
            new AiOrchestratorRequest { TaskType = "DocumentAnalysis" }, CancellationToken.None);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task ExecuteAsync_ThreeProviderChain_StopsOnSuccess()
    {
        var localProvider = CreateMockProvider("Local", isEnabled: true, success: false, errorCategory: "Timeout");
        var openAiProvider = CreateMockProvider("OpenAi", isEnabled: true, success: true);
        var claudeProvider = CreateMockProvider("Claude", isEnabled: true, success: true);

        var options = CreateOptions(new Dictionary<string, List<string>>
        {
            ["QualityAnalysis"] = ["Local", "OpenAi", "Claude"],
        });

        var orchestrator = CreateOrchestrator(options, localProvider, openAiProvider, claudeProvider);

        var result = await orchestrator.ExecuteAsync(
            new AiOrchestratorRequest { TaskType = "QualityAnalysis" }, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal("OpenAi", result.Provider);
    }

    [Fact]
    public async Task ExecuteAsync_MaxFallbackChainRespected()
    {
        var localProvider = CreateMockProvider("Local", isEnabled: true, success: false, errorCategory: "Timeout");
        var claudeProvider = CreateMockProvider("Claude", isEnabled: true, success: false, errorCategory: "NetworkError");
        var openAiProvider = CreateMockProvider("OpenAi", isEnabled: true, success: true);

        // MaxFallbackChain = 2, so only first 2 providers are tried
        var options = CreateOptions(
            new Dictionary<string, List<string>>
            {
                ["DocumentAnalysis"] = ["Local", "Claude", "OpenAi"],
            },
            maxFallbackChain: 2);

        var orchestrator = CreateOrchestrator(options, localProvider, claudeProvider, openAiProvider);

        var result = await orchestrator.ExecuteAsync(
            new AiOrchestratorRequest { TaskType = "DocumentAnalysis" }, CancellationToken.None);

        // OpenAi should NOT have been reached because chain limit is 2
        Assert.False(result.Success);
    }

    [Fact]
    public async Task ExecuteAsync_FallbackUsageRecorded()
    {
        var localProvider = CreateMockProvider("Local", isEnabled: true, success: false, errorCategory: "Timeout");
        var claudeProvider = CreateMockProvider("Claude", isEnabled: true, success: true);

        var options = CreateOptions(new Dictionary<string, List<string>>
        {
            ["DocumentAnalysis"] = ["Local", "Claude"],
        });

        var orchestrator = CreateOrchestrator(options, localProvider, claudeProvider);

        await orchestrator.ExecuteAsync(
            new AiOrchestratorRequest { TaskType = "DocumentAnalysis" }, CancellationToken.None);

        await _usageService.Received(1).RecordUsageAsync(
            Arg.Any<AiProviderResponseDto>(),
            "DocumentAnalysis",
            Arg.Any<Guid?>(),
            Arg.Any<Guid?>(),
            Arg.Any<string?>(),
            true, // wasFallback
            "Local", // fallbackFromProvider
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void GetTaskMappings_UsesProviderRouting()
    {
        var localProvider = CreateMockProvider("Local", isEnabled: true, success: true);
        var claudeProvider = CreateMockProvider("Claude", isEnabled: true, success: true);

        var options = CreateOptions(new Dictionary<string, List<string>>
        {
            ["DocumentAnalysis"] = ["Local", "Claude", "OpenAi"],
        });

        var orchestrator = CreateOrchestrator(options, localProvider, claudeProvider);
        var mappings = orchestrator.GetTaskMappings();

        var docMapping = mappings.FirstOrDefault(m => m.TaskType == "DocumentAnalysis");
        Assert.NotNull(docMapping);
        Assert.Equal("Local", docMapping.PrimaryProvider);
        Assert.Contains("Claude", docMapping.FallbackProvider!);
        Assert.Contains("OpenAi", docMapping.FallbackProvider!);
    }

    [Fact]
    public async Task GetLocalModelsAsync_LocalNotRegistered_ReturnsEmpty()
    {
        // Only Claude registered — no Local provider
        var claudeProvider = CreateMockProvider("Claude", isEnabled: true, success: true);

        var options = CreateOptions(new Dictionary<string, List<string>>
        {
            ["DocumentAnalysis"] = ["Claude"],
        });

        var orchestrator = CreateOrchestrator(options, claudeProvider);
        var models = await orchestrator.GetLocalModelsAsync(CancellationToken.None);

        Assert.Empty(models);
    }

    // ---- Helpers ----

    private static IAiProvider CreateMockProvider(
        string name, bool isEnabled, bool success, string? errorCategory = null)
    {
        var provider = Substitute.For<IAiProvider>();
        provider.ProviderName.Returns(name);
        provider.IsEnabled.Returns(isEnabled);

        var response = new AiProviderResponseDto
        {
            Success = success,
            Provider = name,
            Model = $"{name}-model",
            Content = success ? "Result content" : null,
            ErrorCategory = success ? null : errorCategory,
            ErrorMessage = success ? null : $"{name} failed",
            RequiresHumanReview = true,
            TokenUsage = new AiTokenUsageDto
            {
                InputTokens = 100,
                OutputTokens = 50,
                TotalTokens = 150,
                EstimatedCostUsd = name == "Local" ? 0m : 0.01m,
            },
        };

        provider.AnalyzeAsync(Arg.Any<AiProviderRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(response);
        provider.GenerateRecommendationAsync(Arg.Any<AiProviderRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(response);
        provider.SummarizeAsync(Arg.Any<AiProviderRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(response);
        provider.ExplainAsync(Arg.Any<AiProviderRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(response);
        provider.ExtractStructuredDataAsync(Arg.Any<AiProviderRequestDto>(), Arg.Any<CancellationToken>())
            .Returns(response);

        return provider;
    }

    private static IOptions<AiProviderOptions> CreateOptions(
        Dictionary<string, List<string>> routing,
        int maxFallbackChain = 3)
    {
        return Options.Create(new AiProviderOptions
        {
            ProviderRouting = routing,
            TaskMappings = new Dictionary<string, string>(), // Clear legacy to force ProviderRouting
            FallbackMappings = new Dictionary<string, string>(),
            MaxFallbackChain = maxFallbackChain,
            MaxContextSizeChars = 50_000,
            MaxLocalContextSizeChars = 12_000,
            Local = new LocalProviderOptions { Enabled = true },
            Claude = new ClaudeProviderOptions { Enabled = true },
            OpenAi = new OpenAiProviderOptions { Enabled = true },
        });
    }

    private AiOrchestratorService CreateOrchestrator(
        IOptions<AiProviderOptions> options, params IAiProvider[] providers)
    {
        return new AiOrchestratorService(
            providers,
            _contextService,
            _usageService,
            _promptManager,
            options,
            _logger);
    }
}
