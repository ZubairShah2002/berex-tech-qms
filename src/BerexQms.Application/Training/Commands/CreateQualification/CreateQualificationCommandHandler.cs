using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Training.Entities;
using BerexQms.Domain.Training.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Training.Commands.CreateQualification;

internal sealed class CreateQualificationCommandHandler
    : ICommandHandler<CreateQualificationCommand, Guid>
{
    private readonly IQualificationRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateQualificationCommandHandler(IQualificationRepository repository, ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateQualificationCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.CodeExistsAsync(request.Code, cancellationToken))
            return TrainingErrors.QualificationCodeExists;

        var qualification = Qualification.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.Code,
            request.Name,
            request.Description,
            request.ScopeProductFamily,
            request.ScopeInspectionType,
            request.ScopeProcessArea,
            request.ValidityMonths,
            request.RenewalWindowDays);

        await _repository.AddAsync(qualification, cancellationToken);

        return qualification.Id;
    }
}
