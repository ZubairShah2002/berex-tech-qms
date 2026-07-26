namespace BerexQms.Application.Inspection.DTOs;

public sealed record MeasurementDto(
    Guid Id,
    Guid? ChecklistItemId,
    string CharacteristicName,
    decimal? MeasuredValue,
    string? TextValue,
    string? Unit,
    string Result,
    Guid? EquipmentId,
    string? OperatorId,
    DateTime RecordedAt,
    int SequenceNumber);
