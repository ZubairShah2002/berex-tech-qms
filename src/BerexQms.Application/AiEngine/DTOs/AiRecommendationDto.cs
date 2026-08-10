namespace BerexQms.Application.AiEngine.DTOs;

public sealed record AiRecommendationDto(
    Guid Id,
    string RecommendationType,
    string Title,
    string Description,
    string Severity,
    string? SourceContextIds,
    string RelatedModule,
    string? RelatedEntityId,
    decimal ConfidenceScore,
    string Status,
    string Reason,
    string? SupportingData,
    string? RecommendedAction,
    DateTime? ReviewedAt,
    string? ReviewedBy,
    string? ReviewNotes,
    DateTime CreatedAt,
    DateTime? ModifiedAt);
