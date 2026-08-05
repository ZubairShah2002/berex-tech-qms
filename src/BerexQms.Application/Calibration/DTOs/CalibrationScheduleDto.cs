namespace BerexQms.Application.Calibration.DTOs;

public sealed record CalibrationScheduleDto(
    Guid Id,
    int IntervalDays,
    int LeadTimeDays,
    string LabType,
    string? ProcedureRef,
    DateTime NextDueDate);
