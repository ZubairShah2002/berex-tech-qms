using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.AiEngine.Services;
using BerexQms.SharedKernel.ValueObjects;
using NSubstitute;

namespace BerexQms.Infrastructure.Tests.AiEngine.Services;

/// <summary>
/// Tests for cost optimization metrics in AI usage tracking — Sprint 17.
/// Validates Local vs Cloud breakdown and estimated avoided cost calculation.
/// </summary>
public class AiUsageTrackingCostTests
{
    private readonly IAiUsageRecordRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly AiUsageTrackingService _service;

    public AiUsageTrackingCostTests()
    {
        _repository = Substitute.For<IAiUsageRecordRepository>();
        _tenantContext = Substitute.For<ITenantContext>();
        _tenantContext.CurrentTenantId.Returns(new TenantId(Guid.NewGuid()));
        _service = new AiUsageTrackingService(_repository, _tenantContext);
    }

    [Fact]
    public async Task GetUsageSummaryAsync_NoRecords_ReturnsZeroCostMetrics()
    {
        _repository.ListAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<AiUsageRecord>());

        var summary = await _service.GetUsageSummaryAsync(CancellationToken.None);

        Assert.Equal(0, summary.LocalRequests);
        Assert.Equal(0, summary.CloudRequests);
        Assert.Equal(0, summary.LocalUsagePercent);
        Assert.Equal(0, summary.CloudUsagePercent);
        Assert.Equal(0, summary.EstimatedAvoidedApiCostUsd);
    }

    [Fact]
    public async Task GetUsageSummaryAsync_OnlyLocalRecords_100PercentLocal()
    {
        var records = new List<AiUsageRecord>
        {
            CreateUsageRecord("Local", "qwen3", 100, 50, 0m, true),
            CreateUsageRecord("Local", "qwen3", 200, 100, 0m, true),
        };
        _repository.ListAllAsync(Arg.Any<CancellationToken>()).Returns(records);

        var summary = await _service.GetUsageSummaryAsync(CancellationToken.None);

        Assert.Equal(2, summary.LocalRequests);
        Assert.Equal(0, summary.CloudRequests);
        Assert.Equal(100m, summary.LocalUsagePercent);
        Assert.Equal(0m, summary.CloudUsagePercent);
    }

    [Fact]
    public async Task GetUsageSummaryAsync_MixedProviders_CorrectPercentages()
    {
        var records = new List<AiUsageRecord>
        {
            CreateUsageRecord("Local", "qwen3", 100, 50, 0m, true),
            CreateUsageRecord("Local", "qwen3", 200, 100, 0m, true),
            CreateUsageRecord("Claude", "claude-sonnet", 150, 75, 0.01m, true),
        };
        _repository.ListAllAsync(Arg.Any<CancellationToken>()).Returns(records);

        var summary = await _service.GetUsageSummaryAsync(CancellationToken.None);

        Assert.Equal(2, summary.LocalRequests);
        Assert.Equal(1, summary.CloudRequests);
        Assert.Equal(66.7m, summary.LocalUsagePercent); // 2/3
        Assert.Equal(33.3m, summary.CloudUsagePercent);  // 1/3
    }

    [Fact]
    public async Task GetUsageSummaryAsync_AvoidedCost_CalculatedFromCloudAverage()
    {
        // Cloud: 225 tokens at $0.01 total → $0.01/225 per token ≈ $0.0000444
        // Local: 450 tokens at $0 → avoided cost = 450 * ($0.01/225) = $0.02
        var records = new List<AiUsageRecord>
        {
            CreateUsageRecord("Claude", "claude-sonnet", 150, 75, 0.01m, true),
            CreateUsageRecord("Local", "qwen3", 300, 150, 0m, true),
        };
        _repository.ListAllAsync(Arg.Any<CancellationToken>()).Returns(records);

        var summary = await _service.GetUsageSummaryAsync(CancellationToken.None);

        // Avoided cost = localTokens * (cloudCost / cloudTokens)
        // = 450 * (0.01 / 225) = 450 * 0.0000444... = 0.02
        Assert.True(summary.EstimatedAvoidedApiCostUsd > 0);
        Assert.Equal(0.02m, summary.EstimatedAvoidedApiCostUsd);
    }

    [Fact]
    public async Task GetUsageSummaryAsync_OnlyCloudRecords_ZeroAvoidedCost()
    {
        var records = new List<AiUsageRecord>
        {
            CreateUsageRecord("Claude", "claude-sonnet", 150, 75, 0.01m, true),
            CreateUsageRecord("OpenAi", "gpt-4o", 200, 100, 0.02m, true),
        };
        _repository.ListAllAsync(Arg.Any<CancellationToken>()).Returns(records);

        var summary = await _service.GetUsageSummaryAsync(CancellationToken.None);

        Assert.Equal(0, summary.LocalRequests);
        Assert.Equal(2, summary.CloudRequests);
        Assert.Equal(0m, summary.EstimatedAvoidedApiCostUsd);
    }

    [Fact]
    public async Task GetUsageSummaryAsync_ProviderBreakdown_IncludesSuccessRate()
    {
        var records = new List<AiUsageRecord>
        {
            CreateUsageRecord("Local", "qwen3", 100, 50, 0m, true),
            CreateUsageRecord("Local", "qwen3", 100, 50, 0m, false),
            CreateUsageRecord("Local", "qwen3", 100, 50, 0m, true),
        };
        _repository.ListAllAsync(Arg.Any<CancellationToken>()).Returns(records);

        var summary = await _service.GetUsageSummaryAsync(CancellationToken.None);

        var localUsage = summary.UsageByProvider.FirstOrDefault(p => p.Provider == "Local");
        Assert.NotNull(localUsage);
        Assert.Equal(3, localUsage.RequestCount);
        Assert.Equal(2, localUsage.SuccessCount);
        Assert.Equal(1, localUsage.FailedCount);
        Assert.Equal(66.7m, localUsage.SuccessRate);
    }

    [Fact]
    public async Task GetUsageSummaryAsync_LocalProvider_ZeroCostInBreakdown()
    {
        var records = new List<AiUsageRecord>
        {
            CreateUsageRecord("Local", "qwen3", 100, 50, 0m, true),
        };
        _repository.ListAllAsync(Arg.Any<CancellationToken>()).Returns(records);

        var summary = await _service.GetUsageSummaryAsync(CancellationToken.None);

        var localUsage = summary.UsageByProvider.First(p => p.Provider == "Local");
        Assert.Equal(0m, localUsage.EstimatedCostUsd);
    }

    [Fact]
    public async Task RecordUsageAsync_LocalProvider_RecordsZeroCost()
    {
        var response = new AiProviderResponseDto
        {
            Success = true,
            Provider = "Local",
            Model = "qwen3",
            TokenUsage = new AiTokenUsageDto
            {
                InputTokens = 100,
                OutputTokens = 50,
                TotalTokens = 150,
                EstimatedCostUsd = 0m, // Local always zero
            },
            ProcessingTimeMs = 500,
            RequiresHumanReview = true,
        };

        await _service.RecordUsageAsync(
            response, "DocumentAnalysis", Guid.NewGuid(), null, null,
            false, null, CancellationToken.None);

        await _repository.Received(1).AddAsync(
            Arg.Is<AiUsageRecord>(r =>
                r.Provider == "Local" &&
                r.EstimatedCostUsd == 0m),
            Arg.Any<CancellationToken>());
    }

    // ---- Helpers ----

    private AiUsageRecord CreateUsageRecord(
        string provider, string model,
        int inputTokens, int outputTokens,
        decimal cost, bool success)
    {
        return AiUsageRecord.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            provider,
            model,
            "DocumentAnalysis",
            Guid.NewGuid(),
            inputTokens,
            outputTokens,
            500,
            success,
            success ? null : "Error",
            success ? null : "Error message",
            null,
            null,
            false,
            null,
            cost);
    }
}
