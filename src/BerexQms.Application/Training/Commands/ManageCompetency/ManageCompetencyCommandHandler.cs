using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Training.Entities;
using BerexQms.Domain.Training.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training.Commands.ManageCompetency;

internal sealed class ManageCompetencyCommandHandler
    : ICommandHandler<ManageCompetencyCommand>
{
    private readonly ICompetencyRecordRepository _competencyRepository;
    private readonly IQualificationRepository _qualificationRepository;
    private readonly ITenantContext _tenantContext;

    public ManageCompetencyCommandHandler(
        ICompetencyRecordRepository competencyRepository,
        IQualificationRepository qualificationRepository,
        ITenantContext tenantContext)
    {
        _competencyRepository = competencyRepository;
        _qualificationRepository = qualificationRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result> Handle(ManageCompetencyCommand request, CancellationToken cancellationToken)
    {
        var qualification = await _qualificationRepository.GetByIdAsync(
            request.QualificationId, cancellationToken);
        if (qualification is null)
            return Result.Failure(TrainingErrors.QualificationNotFound);

        var record = await _competencyRepository.GetByEmployeeAndQualificationAsync(
            request.EmployeeId, request.QualificationId, cancellationToken);

        switch (request.Action.ToUpperInvariant())
        {
            case "START_TRAINING":
                if (record is null)
                {
                    record = CompetencyRecord.Create(
                        Guid.NewGuid(),
                        _tenantContext.CurrentTenantId,
                        request.EmployeeId,
                        request.QualificationId);
                    record.StartTraining();
                    await _competencyRepository.AddAsync(record, cancellationToken);
                }
                else
                {
                    record.StartTraining();
                    _competencyRepository.Update(record);
                }
                break;

            case "QUALIFY":
                if (record is null)
                    return Result.Failure(TrainingErrors.CompetencyNotFound);

                record.MarkQualified(
                    request.QualifiedDate ?? DateTime.UtcNow,
                    qualification.ValidityMonths,
                    request.AssessorId,
                    request.EvidenceRef);
                _competencyRepository.Update(record);
                break;

            case "SUSPEND":
                if (record is null)
                    return Result.Failure(TrainingErrors.CompetencyNotFound);

                record.Suspend();
                _competencyRepository.Update(record);
                break;

            case "REVOKE":
                if (record is null)
                    return Result.Failure(TrainingErrors.CompetencyNotFound);

                record.Revoke();
                _competencyRepository.Update(record);
                break;

            case "EXPIRE":
                if (record is null)
                    return Result.Failure(TrainingErrors.CompetencyNotFound);

                record.MarkExpired();
                _competencyRepository.Update(record);
                break;

            default:
                return Result.Failure(TrainingErrors.InvalidAction);
        }

        return Result.Success();
    }
}
