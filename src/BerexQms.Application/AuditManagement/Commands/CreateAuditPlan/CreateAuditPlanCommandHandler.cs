using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AuditManagement.Entities;
using BerexQms.Domain.AuditManagement.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AuditManagement.Commands.CreateAuditPlan;

internal sealed class CreateAuditPlanCommandHandler : ICommandHandler<CreateAuditPlanCommand, Guid>
{
    private readonly IAuditRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateAuditPlanCommandHandler(
        IAuditRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<Guid>> Handle(CreateAuditPlanCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.PlanNameExistsAsync(request.PlanName, request.Year, cancellationToken))
            return AuditErrors.AlreadyExists;

        var plan = AuditPlan.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.PlanName,
            request.Year,
            request.Description,
            request.Scope);

        await _repository.AddAsync(plan, cancellationToken);
        return plan.Id;
    }
}
