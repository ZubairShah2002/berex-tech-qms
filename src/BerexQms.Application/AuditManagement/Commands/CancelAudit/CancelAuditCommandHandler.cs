using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.AuditManagement.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AuditManagement.Commands.CancelAudit;

internal sealed class CancelAuditCommandHandler : ICommandHandler<CancelAuditCommand>
{
    private readonly IAuditRepository _repository;

    public CancelAuditCommandHandler(IAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(CancelAuditCommand request, CancellationToken cancellationToken)
    {
        var plan = await _repository.GetWithAuditsAsync(request.AuditPlanId, cancellationToken);
        if (plan is null)
            return Result.Failure(AuditErrors.NotFound);

        plan.CancelAudit(request.AuditRecordId);
        await _repository.UpdateAsync(plan, cancellationToken);

        return Result.Success();
    }
}
