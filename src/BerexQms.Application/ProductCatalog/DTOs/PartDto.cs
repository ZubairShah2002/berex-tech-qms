namespace BerexQms.Application.ProductCatalog.DTOs;

public sealed record PartDto(
    Guid Id,
    string PartNumber,
    string Name,
    string? Description,
    string? ProductFamily,
    string? Category,
    string SerializationMode,
    string Status,
    string? UnitOfMeasure,
    string? CurrentRevision,
    int RevisionCount,
    DateTime CreatedAt);
