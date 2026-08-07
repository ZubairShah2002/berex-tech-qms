namespace BerexQms.Application.AiEngine.DTOs;

public sealed record AiModelDto(
    Guid Id,
    string Name,
    string Version,
    string Capability,
    string Status,
    string? Description,
    int? TrainingSampleCount,
    DateTime? TrainedAt,
    DateTime? PromotedAt,
    DateTime CreatedAt);
