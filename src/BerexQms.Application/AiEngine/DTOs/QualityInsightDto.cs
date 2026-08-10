namespace BerexQms.Application.AiEngine.DTOs;

public sealed record QualityInsightDto(
    string Category,
    string Title,
    string Description,
    string Severity,
    decimal ConfidenceScore,
    string RelatedModule,
    string? SupportingEvidence,
    DateTime GeneratedAt);
