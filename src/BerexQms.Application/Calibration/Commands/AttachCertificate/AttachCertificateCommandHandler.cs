using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Calibration.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Calibration.Commands.AttachCertificate;

internal sealed class AttachCertificateCommandHandler
    : ICommandHandler<AttachCertificateCommand>
{
    private readonly IEquipmentRepository _repository;

    public AttachCertificateCommandHandler(IEquipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(AttachCertificateCommand request, CancellationToken cancellationToken)
    {
        var equipment = await _repository.GetWithCalibrationsAsync(request.EquipmentId, cancellationToken);
        if (equipment is null)
            return Result.Failure(CalibrationErrors.EquipmentNotFound);

        var record = equipment.Calibrations.FirstOrDefault(c => c.Id == request.CalibrationId);
        if (record is null)
            return Result.Failure(CalibrationErrors.EquipmentNotFound);

        record.AttachCertificate(
            request.IssuingLab,
            request.AccreditationRef,
            request.FileRef,
            request.ValidFrom,
            request.ValidUntil);

        return Result.Success();
    }
}
