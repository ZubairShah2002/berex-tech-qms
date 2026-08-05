using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Training.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training.Commands.UpdateQualification;

internal sealed class UpdateQualificationCommandHandler
    : ICommandHandler<UpdateQualificationCommand>
{
    private readonly IQualificationRepository _repository;

    public UpdateQualificationCommandHandler(IQualificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(UpdateQualificationCommand request, CancellationToken cancellationToken)
    {
        var qualification = await _repository.GetByIdAsync(request.QualificationId, cancellationToken);
        if (qualification is null)
            return Result.Failure(TrainingErrors.QualificationNotFound);

        qualification.Update(
            request.Name,
            request.Description,
            request.ScopeProductFamily,
            request.ScopeInspectionType,
            request.ScopeProcessArea,
            request.ValidityMonths,
            request.RenewalWindowDays);

        return Result.Success();
    }
}
