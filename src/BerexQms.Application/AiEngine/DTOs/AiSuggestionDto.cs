namespace BerexQms.Application.AiEngine.DTOs;

public sealed record AiSuggestionDto(
    Guid InteractionId,
    string Capability,
    string? OutputSummary,
    decimal? ConfidenceScore,
    string? ConfidenceLevel,
    IReadOnlyList<AiSourceReferenceDto> SourceReferences,
    bool IsSuppressed);
