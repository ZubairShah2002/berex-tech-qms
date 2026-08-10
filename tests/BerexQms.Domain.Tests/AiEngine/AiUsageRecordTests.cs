using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Tests.AiEngine;

public class AiUsageRecordTests
{
    private static readonly TenantId TestTenantId = TenantId.From(Guid.NewGuid());

    [Fact]
    public void Create_ValidParameters_SetsAllProperties()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var recId = Guid.NewGuid();

        var record = AiUsageRecord.Create(
            id, TestTenantId,
            "Claude", "claude-sonnet-4-20250514",
            "DocumentAnalysis", userId,
            inputTokens: 500, outputTokens: 200,
            processingTimeMs: 3200,
            success: true,
            errorCategory: null, errorDetail: null,
            recommendationId: recId,
            contextDocumentIds: "doc-1,doc-2",
            wasFallback: false,
            fallbackFromProvider: null,
            estimatedCostUsd: 0.0045m);

        Assert.Equal(id, record.Id);
        Assert.Equal(TestTenantId, record.TenantId);
        Assert.Equal("Claude", record.Provider);
        Assert.Equal("claude-sonnet-4-20250514", record.Model);
        Assert.Equal("DocumentAnalysis", record.TaskType);
        Assert.Equal(userId, record.UserId);
        Assert.Equal(500, record.InputTokens);
        Assert.Equal(200, record.OutputTokens);
        Assert.Equal(700, record.TotalTokens);
        Assert.Equal(0.0045m, record.EstimatedCostUsd);
        Assert.Equal(3200, record.ProcessingTimeMs);
        Assert.True(record.Success);
        Assert.Null(record.ErrorCategory);
        Assert.Null(record.ErrorDetail);
        Assert.Equal(recId, record.RecommendationId);
        Assert.Equal("doc-1,doc-2", record.ContextDocumentIds);
        Assert.False(record.WasFallback);
        Assert.Null(record.FallbackFromProvider);
    }

    [Fact]
    public void Create_TotalTokens_CalculatedFromInputPlusOutput()
    {
        var record = AiUsageRecord.Create(
            Guid.NewGuid(), TestTenantId,
            "OpenAI", "gpt-4o", "Summarization", null,
            inputTokens: 1000, outputTokens: 300,
            processingTimeMs: 2500, success: true,
            null, null, null, null, false, null, null);

        Assert.Equal(1300, record.TotalTokens);
    }

    [Fact]
    public void Create_FailedRequest_RecordsErrorDetails()
    {
        var record = AiUsageRecord.Create(
            Guid.NewGuid(), TestTenantId,
            "Claude", "claude-sonnet-4-20250514", "RiskAnalysis", null,
            inputTokens: 400, outputTokens: 0,
            processingTimeMs: 30000, success: false,
            errorCategory: "Timeout", errorDetail: "Request timed out after 30s.",
            null, null, false, null, null);

        Assert.False(record.Success);
        Assert.Equal("Timeout", record.ErrorCategory);
        Assert.Equal("Request timed out after 30s.", record.ErrorDetail);
        Assert.Equal(0, record.OutputTokens);
    }

    [Fact]
    public void Create_FallbackRequest_RecordsFallbackInfo()
    {
        var record = AiUsageRecord.Create(
            Guid.NewGuid(), TestTenantId,
            "OpenAI", "gpt-4o", "CAPAAnalysis", null,
            inputTokens: 600, outputTokens: 250,
            processingTimeMs: 4000, success: true,
            null, null, null, null,
            wasFallback: true,
            fallbackFromProvider: "Claude",
            estimatedCostUsd: 0.0032m);

        Assert.True(record.WasFallback);
        Assert.Equal("Claude", record.FallbackFromProvider);
        Assert.Equal("OpenAI", record.Provider);
    }

    [Fact]
    public void Create_NullOptionalFields_SetsNullValues()
    {
        var record = AiUsageRecord.Create(
            Guid.NewGuid(), TestTenantId,
            "Claude", "claude-sonnet-4-20250514", "QualityAnalysis",
            userId: null,
            inputTokens: 100, outputTokens: 50,
            processingTimeMs: 1000, success: true,
            null, null, null, null, false, null, null);

        Assert.Null(record.UserId);
        Assert.Null(record.ErrorCategory);
        Assert.Null(record.ErrorDetail);
        Assert.Null(record.RecommendationId);
        Assert.Null(record.ContextDocumentIds);
        Assert.Null(record.FallbackFromProvider);
        Assert.Null(record.EstimatedCostUsd);
    }
}
