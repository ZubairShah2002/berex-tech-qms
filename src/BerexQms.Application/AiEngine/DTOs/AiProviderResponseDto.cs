namespace BerexQms.Application.AiEngine.DTOs;

/// <summary>
/// Normalized AI response contract. All provider responses are mapped into
/// this structure before the application consumes them. Responses must be
/// validated before being used — AI output is untrusted input.
/// </summary>
public sealed record AiProviderResponseDto
{
    public bool Success { get; init; }
    public string Provider { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string? Content { get; init; }
    public decimal ConfidenceScore { get; init; }
    public string? ReasoningSummary { get; init; }
    public IReadOnlyList<AiRecommendationItemDto> Recommendations { get; init; } = [];
    public string? SupportingContextIds { get; init; }
    public AiTokenUsageDto TokenUsage { get; init; } = new();
    public long ProcessingTimeMs { get; init; }
    public bool RequiresHumanReview { get; init; } = true;
    public string? ErrorMessage { get; init; }
    public string? ErrorCategory { get; init; }
}

/// <summary>
/// Individual recommendation from structured AI output.
/// </summary>
public sealed record AiRecommendationItemDto
{
    public string RecommendationType { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public string? SupportingEvidence { get; init; }
    public decimal ConfidenceScore { get; init; }
    public string? RecommendedAction { get; init; }
    public bool RequiresHumanReview { get; init; } = true;
    public string ConfidenceCategory { get; init; } = "Recommendation";
}

/// <summary>
/// Token usage from a single AI provider call.
/// </summary>
public sealed record AiTokenUsageDto
{
    public int InputTokens { get; init; }
    public int OutputTokens { get; init; }
    public int TotalTokens { get; init; }
    public decimal? EstimatedCostUsd { get; init; }
}
