using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Capa.DTOs;
using BerexQms.Domain.Capa.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Capa.Commands.ScheduleVerification;

public sealed class ScheduleVerificationCommandHandler
    : ICommandHandler<ScheduleVerificationCommand, EffectivenessVerificationDto>
{
    private readonly ICAPARepository _repository;

    public ScheduleVerificationCommandHandler(ICAPARepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<EffectivenessVerificationDto>> Handle(
        ScheduleVerificationCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetWithVerificationsAsync(request.CapaId, cancellationToken);
        if (record is null)
            return CAPAErrors.NotFound;

        var verification = record.ScheduleVerification(request.ScheduledDate, request.VerificationCriteria);
        await _repository.UpdateAsync(record, cancellationToken);

        return new EffectivenessVerificationDto(
            verification.Id,
            verification.ScheduledDate,
            verification.VerificationCriteria,
            verification.VerifierId,
            verification.Result,
            verification.Evidence,
            verification.IsEffective,
            verification.VerifiedAt,
            verification.CreatedAt);
    }
}
