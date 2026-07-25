namespace BerexQms.Application.Inspection.DTOs;

public sealed record ChecklistDto(
    Guid Id,
    Guid PartRevisionId,
    string RevisionCode,
    DateTime SnapshotAt,
    IReadOnlyList<ChecklistItemDto> Items);
