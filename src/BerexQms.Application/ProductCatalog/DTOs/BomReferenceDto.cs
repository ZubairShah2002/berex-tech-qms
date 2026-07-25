namespace BerexQms.Application.ProductCatalog.DTOs;

public sealed record BomReferenceDto(
    Guid Id,
    Guid ChildPartId,
    string ChildPartNumber,
    string ChildPartName,
    decimal Quantity,
    string? ReferenceDesignator,
    int SortOrder);
