using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Calibration.DTOs;
using BerexQms.Domain.Calibration.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Calibration.Commands.RecordGaugeStudy;

internal sealed class RecordGaugeStudyCommandHandler
    : ICommandHandler<RecordGaugeStudyCommand, GaugeStudyDto>
{
    private readonly IEquipmentRepository _repository;

    public RecordGaugeStudyCommandHandler(IEquipmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<GaugeStudyDto>> Handle(
        RecordGaugeStudyCommand request, CancellationToken cancellationToken)
    {
        var equipment = await _repository.GetByIdAsync(request.EquipmentId, cancellationToken);
        if (equipment is null)
            return CalibrationErrors.EquipmentNotFound;

        var study = equipment.RecordGaugeStudy(
            request.CharacteristicId,
            request.StudyDate,
            request.TotalGRRPct,
            request.RepeatabilityPct,
            request.ReproducibilityPct,
            request.PartVariationPct,
            request.Ndc);

        return new GaugeStudyDto(
            study.Id,
            study.CharacteristicId,
            study.StudyDate,
            study.TotalGRRPct,
            study.RepeatabilityPct,
            study.ReproducibilityPct,
            study.PartVariationPct,
            study.Ndc,
            study.Result);
    }
}
