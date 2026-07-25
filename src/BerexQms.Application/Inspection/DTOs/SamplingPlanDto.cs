namespace BerexQms.Application.Inspection.DTOs;

public sealed record SamplingPlanDto(
    Guid Id,
    Guid PartId,
    Guid? SupplierId,
    string InspectionType,
    string Level,
    decimal AqlValue,
    int SampleSize,
    int AcceptNumber,
    int RejectNumber,
    bool IsActive,
    DateTime CreatedAt);
