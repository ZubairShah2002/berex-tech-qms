namespace BerexQms.Application.Inspection.DTOs;

public sealed record ChecklistItemDto(
    Guid Id,
    string CharacteristicName,
    string? SpecificationLimit,
    decimal? NominalValue,
    decimal? UpperLimit,
    decimal? LowerLimit,
    string? Unit,
    bool IsCritical,
    int SortOrder);
