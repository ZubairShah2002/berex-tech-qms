using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;

namespace BerexQms.Application.Calibration.Commands.SetSchedule;

public sealed record SetScheduleCommand(
    Guid EquipmentId,
    int IntervalDays,
    int LeadTimeDays,
    string LabType,
    string? ProcedureRef,
    DateTime NextDueDate) : ICommand<CalibrationScheduleDto>;
