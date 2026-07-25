using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Inspection.DTOs;
using BerexQms.Domain.Inspection.Enums;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection.Commands.RecordMeasurement;

public sealed class RecordMeasurementCommandHandler
    : ICommandHandler<RecordMeasurementCommand, MeasurementDto>
{
    private readonly IInspectionRepository _inspectionRepository;

    public RecordMeasurementCommandHandler(IInspectionRepository inspectionRepository)
    {
        _inspectionRepository = inspectionRepository;
    }

    public async Task<Result<MeasurementDto>> Handle(
        RecordMeasurementCommand request, CancellationToken cancellationToken)
    {
        var record = await _inspectionRepository.GetWithMeasurementsAsync(
            request.InspectionId, cancellationToken);
        if (record is null)
            return InspectionErrors.NotFound;

        if (!Enum.TryParse<MeasurementResult>(request.Result, true, out var measurementResult))
            return InspectionErrors.InvalidMeasurementResult;

        var measurement = record.AddMeasurement(
            request.ChecklistItemId,
            request.CharacteristicName,
            request.MeasuredValue,
            request.TextValue,
            request.Unit,
            measurementResult,
            request.EquipmentId,
            request.OperatorId);

        await _inspectionRepository.UpdateAsync(record, cancellationToken);

        return new MeasurementDto(
            measurement.Id,
            measurement.ChecklistItemId,
            measurement.CharacteristicName,
            measurement.MeasuredValue,
            measurement.TextValue,
            measurement.Unit,
            measurement.Result.ToString(),
            measurement.EquipmentId,
            measurement.OperatorId,
            measurement.RecordedAt,
            measurement.SequenceNumber);
    }
}
