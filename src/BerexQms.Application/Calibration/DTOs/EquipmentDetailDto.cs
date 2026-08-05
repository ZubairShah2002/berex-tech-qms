namespace BerexQms.Application.Calibration.DTOs;

public sealed record EquipmentDetailDto(
    Guid Id,
    string Code,
    string Name,
    string? Type,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    string Status,
    string? Location,
    string? Department,
    string? Area,
    Guid? CustodianId,
    CalibrationScheduleDto? Schedule,
    IReadOnlyList<CalibrationRecordDto> Calibrations,
    IReadOnlyList<GaugeStudyDto> GaugeStudies,
    IReadOnlyList<ImpactAssessmentDto> ImpactAssessments,
    DateTime CreatedAt);
