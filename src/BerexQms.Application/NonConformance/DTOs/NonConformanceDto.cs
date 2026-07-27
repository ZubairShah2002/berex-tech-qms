namespace BerexQms.Application.NonConformance.DTOs;

public sealed record NonConformanceDto(
    Guid Id,
    string NcrNumber,
    string Status,
    string Severity,
    string Source,
    string DetectionPoint,
    Guid PartId,
    string? LotNumber,
    Guid? SupplierId,
    int QuantityAffected,
    int QuantityDefective,
    string? AssignedTo,
    DateTime CreatedAt);
