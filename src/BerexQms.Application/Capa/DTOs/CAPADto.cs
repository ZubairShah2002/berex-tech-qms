namespace BerexQms.Application.Capa.DTOs;

public sealed record CAPADto(
    Guid Id,
    string CapaNumber,
    string Title,
    string Status,
    string Priority,
    string SourceType,
    string OwnerId,
    string? AssignedTo,
    Guid? SourceNonConformanceId,
    DateTime? TargetClosureDate,
    int ActionCount,
    int CompletedActionCount,
    DateTime CreatedAt);
