namespace BerexQms.Application.ProductCatalog.DTOs;

public sealed record SpecificationParameterDto(
    Guid Id,
    string Name,
    string Type,
    string? Unit,
    decimal? NominalValue,
    decimal? UpperTolerance,
    decimal? LowerTolerance,
    string? TextValue,
    bool IsCritical,
    int SortOrder);
