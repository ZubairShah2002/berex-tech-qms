namespace BerexQms.Application.AiEngine.DTOs;

/// <summary>
/// Aggregated AI usage statistics for dashboard reporting.
/// Includes cost optimization metrics for Local vs Cloud providers.
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

    // ---- Cost optimization metrics (Sprint 17) ----

    /// <summary>Number of requests handled by the Local provider (zero API cost).</summary>
    public int LocalRequests { get; init; }

    /// <summary>Number of requests handled by cloud providers (paid).</summary>
    public int CloudRequests { get; init; }

    /// <summary>Percentage of total requests handled locally.</summary>
    public decimal LocalUsagePercent { get; init; }

    /// <summary>Percentage of total requests handled by cloud providers.</summary>
    public decimal CloudUsagePercent { get; init; }

    /// <summary>
    /// Estimated API cost that would have been incurred if Local requests
    /// had been served by the cheapest cloud provider instead.
    /// Clearly labeled as an estimate — does not include infrastructure costs.
    /// </summary>
    public decimal EstimatedAvoidedApiCostUsd { get; init; }
}

public sealed record AiUsageByProviderDto
{
    public string Provider { get; init; } = string.Empty;
    public int RequestCount { get; init; }
    public int TotalTokens { get; init; }
    public decimal EstimatedCostUsd { get; init; }
    public long AverageProcessingTimeMs { get; init; }
    public int FailedCount { get; init; }
    public int SuccessCount { get; init; }
    public decimal SuccessRate { get; init; }
    public int FallbackCount { get; init; }
}

public sealed record AiUsageByTaskTypeDto
{
    public string TaskType { get; init; } = string.Empty;
    public int RequestCount { get; init; }
    public int TotalTokens { get; init; }
    public long AverageProcessingTimeMs { get; init; }
}
