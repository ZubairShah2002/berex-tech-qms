namespace BerexQms.Application.ProductCatalog.DTOs;

public sealed record PartDetailDto(
    Guid Id,
    string PartNumber,
    string Name,
    string? Description,
    string? ProductFamily,
    string? Category,
    string SerializationMode,
    string Status,
    string? UnitOfMeasure,
    IReadOnlyList<PartRevisionDto> Revisions,
    IReadOnlyList<BomReferenceDto> BomReferences,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? ModifiedAt);
