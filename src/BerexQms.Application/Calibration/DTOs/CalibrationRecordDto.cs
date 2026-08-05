namespace BerexQms.Application.Calibration.DTOs;

public sealed record CalibrationRecordDto(
    Guid Id,
    DateTime CalibrationDate,
    string Result,
    Guid? TechnicianId,
    string? ProcedureRef,
    string? Notes,
    string? EnvironmentalConditions,
    DateTime? NextDueDate,
    CertificateDto? Certificate);

public sealed record CertificateDto(
    string IssuingLab,
    string? AccreditationRef,
    string? FileRef,
    DateTime ValidFrom,
    DateTime ValidUntil);
