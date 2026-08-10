namespace BerexQms.Application.AiEngine.DTOs;

/// <summary>
/// Aggregated AI usage statistics for dashboard reporting.
/// </summary>
public sealed record AiUsageSummaryDto
{
    public int TotalRequests { get; init; }
    public int SuccessfulRequests { get; init; }
    public int FailedRequests { get; init; }
    public int FallbackRequests { get; init; }
    public int TotalInputTokens { get; init; }
    public int TotalOutputTokens { get; init; }
    public decimal TotalEstimatedCostUsd { get; init; }
    public long AverageProcessingTimeMs { get; init; }
    public IReadOnlyList<AiUsageByProviderDto> UsageByProvider { get; init; } = [];
    public IReadOnlyList<AiUsageByTaskTypeDto> UsageByTaskType { get; init; } = [];
}

public sealed record AiUsageByProviderDto
{
    public string Provider { get; init; } = string.Empty;
    public int RequestCount { get; init; }
    public int TotalTokens { get; init; }
    public decimal EstimatedCostUsd { get; init; }
    public long AverageProcessingTimeMs { get; init; }
    public int FailedCount { get; init; }
}

public sealed record AiUsageByTaskTypeDto
{
    public string TaskType { get; init; } = string.Empty;
    public int RequestCount { get; init; }
    public int TotalTokens { get; init; }
    public long AverageProcessingTimeMs { get; init; }
}
