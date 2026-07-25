namespace BerexQms.Application.ProductCatalog.DTOs;

public sealed record PartRevisionDto(
    Guid Id,
    string RevisionCode,
    string Status,
    string? Description,
    string? ChangeReason,
    DateTime? ReleasedAt,
    string? ReleasedBy,
    DateTime? ObsoletedAt,
    IReadOnlyList<SpecificationParameterDto> SpecificationParameters,
    DateTime CreatedAt);
