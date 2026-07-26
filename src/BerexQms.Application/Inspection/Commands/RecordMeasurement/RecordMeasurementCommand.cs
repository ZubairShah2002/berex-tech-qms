using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Inspection.DTOs;

namespace BerexQms.Application.Inspection.Commands.RecordMeasurement;

public sealed record RecordMeasurementCommand(
    Guid InspectionId,
    Guid? ChecklistItemId,
    string CharacteristicName,
    decimal? MeasuredValue,
    string? TextValue,
    string? Unit,
    string Result,
    Guid? EquipmentId,
    string? OperatorId) : ICommand<MeasurementDto>;
