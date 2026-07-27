namespace BerexQms.Application.NonConformance.DTOs;

public sealed record SimilarNcDto(
    Guid Id,
    string NcrNumber,
    string Status,
    string Severity,
    string? DefectType,
    Guid PartId,
    Guid? SupplierId,
    DateTime CreatedAt);
