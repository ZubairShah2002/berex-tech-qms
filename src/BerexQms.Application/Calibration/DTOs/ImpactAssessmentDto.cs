namespace BerexQms.Application.Calibration.DTOs;

public sealed record ImpactAssessmentDto(
    Guid Id,
    Guid EquipmentId,
    Guid FailedCalibrationId,
    DateTime AffectedFrom,
    DateTime AffectedTo,
    int AffectedInspectionCount,
    string Status,
    Guid? ReviewedBy,
    string? Notes);
