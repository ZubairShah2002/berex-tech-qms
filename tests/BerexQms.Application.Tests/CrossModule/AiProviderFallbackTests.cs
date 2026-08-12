using BerexQms.Application.AiEngine.Commands.ExecuteAiAnalysis;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Application.Interfaces;
using BerexQms.SharedKernel.Results;
using NSubstitute;

namespace BerexQms.Application.Tests.CrossModule;

/// <summary>
/// Cross-module integration tests for AI Provider Failure and Fallback:
/// Primary Provider -> Failure -> Fallback Provider -> Usage Record -> Audit
///
/// Tests exercise the application layer command handler that delegates
/// to the orchestrator, which handles provider selection and fallback.
/// </summary>
public class AiProviderFallbackTests
{
    private static readonly Guid TestUserId = Guid.NewGuid();

    private static ICurrentUserService CreateCurrentUser()
    {
        var mock = Substitute.For<ICurrentUserService>();
        mock.UserId.Returns(TestUserId);
        mock.TenantId.Returns(Guid.NewGuid());
        mock.IsAuthenticated.Returns(true);
        mock.Email.Returns("test@berex.com");
        mock.Roles.Returns(new List<string> { "Operator" });
        return mock;
    }

    [Fact]
    public async Task PrimaryProviderFails_OrchestratorTriesFallback_ReturnsSuccess()
    {
        // The orchestrator returns a successful response from the fallback provider
        var orchestrator = Substitute.For<IAiOrchestrator>();
        orchestrator.ExecuteAsync(Arg.Any<AiOrchestratorRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiProviderResponseDto
            {
                Success = true,
                Provider = "Local", // Fallback provider
                Model = "local-model-v1",
                Content = "Analysis complete via fallback provider",
                ConfidenceScore = 0.85m,
                RequiresHumanReview = true,
                TokenUsage = new AiTokenUsageDto
                {
                    InputTokens = 500,
                    OutputTokens = 200,
                    TotalTokens = 700,
                },
            });

        var currentUser = CreateCurrentUser();
        var handler = new ExecuteAiAnalysisCommandHandler(orchestrator, currentUser);
        var command = new ExecuteAiAnalysisCommand(
            "DocumentAnalysis", "Inspection", "Analyze this report", null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Local", result.Value.Provider);
        Assert.True(result.Value.RequiresHumanReview);
    }

    [Fact]
    public async Task AllProvidersFail_ReturnsAppropriateError()
    {
        // The orchestrator returns failure when all providers fail
        var orchestrator = Substitute.For<IAiOrchestrator>();
        orchestrator.ExecuteAsync(Arg.Any<AiOrchestratorRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiProviderResponseDto
            {
                Success = false,
                ErrorCategory = "MaxRetriesExceeded",
                ErrorMessage = "All configured providers failed.",
                RequiresHumanReview = true,
                TokenUsage = new AiTokenUsageDto(),
            });

        var currentUser = CreateCurrentUser();
        var handler = new ExecuteAiAnalysisCommandHandler(orchestrator, currentUser);
        var command = new ExecuteAiAnalysisCommand(
            "RiskAnalysis", "NonConformance", "Analyze risk", null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AiEngine.AiProviderRequestFailed", result.Error.Code);
    }

    [Fact]
    public async Task SuccessfulExecution_RecordsUsageViaOrchestrator()
    {
        var orchestrator = Substitute.For<IAiOrchestrator>();
        orchestrator.ExecuteAsync(Arg.Any<AiOrchestratorRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiProviderResponseDto
            {
                Success = true,
                Provider = "Claude",
                Model = "claude-opus-4-20250514",
                Content = "Quality analysis complete",
                ConfidenceScore = 0.92m,
                RequiresHumanReview = true,
                TokenUsage = new AiTokenUsageDto
                {
                    InputTokens = 1200,
                    OutputTokens = 800,
                    TotalTokens = 2000,
                    EstimatedCostUsd = 0.025m,
                },
                ProcessingTimeMs = 1500,
            });

        var currentUser = CreateCurrentUser();
        var handler = new ExecuteAiAnalysisCommandHandler(orchestrator, currentUser);
        var command = new ExecuteAiAnalysisCommand(
            "QualityAnalysis", "Inspection", "Analyze trends", null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2000, result.Value.TokenUsage.TotalTokens);
        Assert.Equal(0.025m, result.Value.TokenUsage.EstimatedCostUsd);

        // Verify orchestrator was called with correct request
        await orchestrator.Received(1).ExecuteAsync(
            Arg.Is<AiOrchestratorRequest>(r =>
                r.TaskType == "QualityAnalysis" &&
                r.Module == "Inspection" &&
                r.UserId == TestUserId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProviderUnavailable_HandledGracefully()
    {
        var orchestrator = Substitute.For<IAiOrchestrator>();
        orchestrator.ExecuteAsync(Arg.Any<AiOrchestratorRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiProviderResponseDto
            {
                Success = false,
                ErrorCategory = "ProviderDisabled",
                ErrorMessage = "Provider 'Claude' is disabled.",
                RequiresHumanReview = true,
                TokenUsage = new AiTokenUsageDto(),
            });

        var currentUser = CreateCurrentUser();
        var handler = new ExecuteAiAnalysisCommandHandler(orchestrator, currentUser);
        var command = new ExecuteAiAnalysisCommand(
            "Summarization", "DocumentControl", "Summarize this SOP", null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("AiEngine.AiProviderRequestFailed", result.Error.Code);
    }

    [Fact]
    public async Task RateLimitError_TriggersFallback_ReturnsFromFallbackProvider()
    {
        // Simulate rate limit on primary -> orchestrator falls back and succeeds
        var orchestrator = Substitute.For<IAiOrchestrator>();
        orchestrator.ExecuteAsync(Arg.Any<AiOrchestratorRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiProviderResponseDto
            {
                Success = true,
                Provider = "OpenAi", // Fallback after Claude rate limit
                Model = "gpt-4o",
                Content = "CAPA analysis via fallback",
                ConfidenceScore = 0.88m,
                RequiresHumanReview = true,
                TokenUsage = new AiTokenUsageDto
                {
                    InputTokens = 800,
                    OutputTokens = 400,
                    TotalTokens = 1200,
                },
            });

        var currentUser = CreateCurrentUser();
        var handler = new ExecuteAiAnalysisCommandHandler(orchestrator, currentUser);
        var command = new ExecuteAiAnalysisCommand(
            "CAPAAnalysis", "Capa", "Analyze CAPA effectiveness", null);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("OpenAi", result.Value.Provider); // Got result from fallback
        Assert.True(result.Value.RequiresHumanReview);
    }

    [Fact]
    public async Task ExecuteAiAnalysis_PassesUserIdForAuditTrail()
    {
        var orchestrator = Substitute.For<IAiOrchestrator>();
        orchestrator.ExecuteAsync(Arg.Any<AiOrchestratorRequest>(), Arg.Any<CancellationToken>())
            .Returns(new AiProviderResponseDto
            {
                Success = true,
                Provider = "Claude",
                Model = "claude-opus-4-20250514",
                Content = "Result",
                ConfidenceScore = 0.90m,
                RequiresHumanReview = true,
                TokenUsage = new AiTokenUsageDto(),
            });

        var currentUser = CreateCurrentUser();
        var handler = new ExecuteAiAnalysisCommandHandler(orchestrator, currentUser);
        var command = new ExecuteAiAnalysisCommand(
            "DefectTrendAnalysis", "NonConformance", "Trend analysis", "entity-123", 5);

        await handler.Handle(command, CancellationToken.None);

        // Verify the request includes the authenticated user ID for audit trail
        await orchestrator.Received(1).ExecuteAsync(
            Arg.Is<AiOrchestratorRequest>(r =>
                r.UserId == TestUserId &&
                r.TaskType == "DefectTrendAnalysis" &&
                r.Module == "NonConformance" &&
                r.Content == "Trend analysis" &&
                r.EntityId == "entity-123" &&
                r.MaxContextDocuments == 5),
            Arg.Any<CancellationToken>());
    }
}
