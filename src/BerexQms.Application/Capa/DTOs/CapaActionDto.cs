namespace BerexQms.Application.Capa.DTOs;

public sealed record CapaActionDto(
    Guid Id,
    string ActionType,
    string Description,
    string OwnerId,
    DateTime DueDate,
    string? EvidenceRequirement,
    string? CompletionNotes,
    string? EvidenceProvided,
    DateTime? CompletedAt,
    string? CompletedBy,
    bool IsOverdue,
    DateTime CreatedAt);
