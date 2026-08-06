namespace BerexQms.Application.AiEngine.DTOs;

public sealed record AiInteractionDetailDto(
    Guid Id,
    string Capability,
    Guid UserId,
    string ModelId,
    string? OutputSummary,
    decimal? ConfidenceScore,
    string? ConfidenceLevel,
    IReadOnlyList<AiSourceReferenceDto> SourceReferences,
    string Status,
    string? UserAction,
    string? UserJustification,
    DateTime RequestedAt,
    DateTime? CompletedAt,
    int? ResponseTimeMs,
    string? InputSummary,
    string CreatedBy,
    DateTime CreatedAt);
