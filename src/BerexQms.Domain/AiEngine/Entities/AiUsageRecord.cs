using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AiEngine.Entities;

/// <summary>
/// Records AI provider usage for cost tracking and audit.
/// Captures provider, model, token usage, duration, and outcome.
/// </summary>
public sealed class AiUsageRecord : AggregateRoot<Guid>, IAuditableEntity
{
    public string Provider { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public string TaskType { get; private set; } = string.Empty;
    public Guid? UserId { get; private set; }
    public int InputTokens { get; private set; }
    public int OutputTokens { get; private set; }
    public int TotalTokens { get; private set; }
    public decimal? EstimatedCostUsd { get; private set; }
    public long ProcessingTimeMs { get; private set; }
    public bool Success { get; private set; }
    public string? ErrorCategory { get; private set; }
    public string? ErrorDetail { get; private set; }
    public Guid? RecommendationId { get; private set; }
    public string? ContextDocumentIds { get; private set; }
    public bool WasFallback { get; private set; }
    public string? FallbackFromProvider { get; private set; }

    // IAuditableEntity
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private AiUsageRecord() { }

    public static AiUsageRecord Create(
        Guid id,
        TenantId tenantId,
        string provider,
        string model,
        string taskType,
        Guid? userId,
        int inputTokens,
        int outputTokens,
        long processingTimeMs,
        bool success,
        string? errorCategory,
        string? errorDetail,
        Guid? recommendationId,
        string? contextDocumentIds,
        bool wasFallback,
        string? fallbackFromProvider,
        decimal? estimatedCostUsd)
    {
        var record = new AiUsageRecord
        {
            Id = id,
            TenantId = tenantId,
            Provider = provider,
            Model = model,
            TaskType = taskType,
            UserId = userId,
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            TotalTokens = inputTokens + outputTokens,
            EstimatedCostUsd = estimatedCostUsd,
            ProcessingTimeMs = processingTimeMs,
            Success = success,
            ErrorCategory = errorCategory,
            ErrorDetail = errorDetail,
            RecommendationId = recommendationId,
            ContextDocumentIds = contextDocumentIds,
            WasFallback = wasFallback,
            FallbackFromProvider = fallbackFromProvider,
        };

        return record;
    }
}
