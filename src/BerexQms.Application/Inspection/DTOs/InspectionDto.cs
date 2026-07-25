namespace BerexQms.Application.Inspection.DTOs;

public sealed record InspectionDto(
    Guid Id,
    string InspectionNumber,
    string Type,
    string Status,
    Guid PartId,
    Guid? PartRevisionId,
    string? LotNumber,
    int? LotSize,
    int? SampleSize,
    Guid? SupplierId,
    string InspectorId,
    string? Result,
    DateTime CreatedAt);
