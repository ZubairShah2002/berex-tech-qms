using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;

namespace BerexQms.Application.Calibration.Commands.RecordCalibration;

public sealed record RecordCalibrationCommand(
    Guid EquipmentId,
    DateTime CalibrationDate,
    string Result,
    Guid? TechnicianId,
    string? ProcedureRef,
    string? Notes,
    string? EnvironmentalConditions) : ICommand<CalibrationRecordDto>;
