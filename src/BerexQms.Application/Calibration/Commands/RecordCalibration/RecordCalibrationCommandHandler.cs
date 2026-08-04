using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;
using BerexQms.Domain.Calibration.Enums;
using BerexQms.Domain.Calibration.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Calibration.Commands.RecordCalibration;

internal sealed class RecordCalibrationCommandHandler
    : ICommandHandler<RecordCalibrationCommand, CalibrationRecordDto>
{
    private readonly IEquipmentRepository _repository;

    public RecordCalibrationCommandHandler(IEquipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<CalibrationRecordDto>> Handle(
        RecordCalibrationCommand request, CancellationToken cancellationToken)
    {
        var equipment = await _repository.GetWithCalibrationsAsync(request.EquipmentId, cancellationToken);
        if (equipment is null)
            return CalibrationErrors.EquipmentNotFound;

        if (!Enum.TryParse<CalibrationResult>(request.Result, true, out var result))
            return CalibrationErrors.InvalidStatus;

        var record = equipment.RecordCalibration(
            request.CalibrationDate,
            result,
            request.TechnicianId,
            request.ProcedureRef,
            request.Notes,
            request.EnvironmentalConditions);

        CertificateDto? certDto = null;
        if (record.Certificate is not null)
        {
            var c = record.Certificate;
            certDto = new CertificateDto(c.IssuingLab, c.AccreditationRef, c.FileRef, c.ValidFrom, c.ValidUntil);
        }

        return new CalibrationRecordDto(
            record.Id,
            record.CalibrationDate,
            record.Result,
            record.TechnicianId,
            record.ProcedureRef,
            record.Notes,
            record.EnvironmentalConditions,
            record.NextDueDate,
            certDto);
    }
}
