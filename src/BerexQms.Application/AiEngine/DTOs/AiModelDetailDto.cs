namespace BerexQms.Application.AiEngine.DTOs;

public sealed record AiModelDetailDto(
    Guid Id,
    string Name,
    string Version,
    string Capability,
    string Status,
    string? Description,
    int? TrainingSampleCount,
    DateTime? TrainedAt,
    DateTime? PromotedAt,
    DateTime CreatedAt,
    string? TrainingMetrics,
    string? ValidationMetrics,
    string? HyperParameters,
    string? DataSnapshotReference,
    DateTime? RetiredAt,
    string CreatedBy);
