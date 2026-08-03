using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.AuditManagement.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AuditManagement.Commands.StartAudit;

internal sealed class StartAuditCommandHandler : ICommandHandler<StartAuditCommand>
{
    private readonly IAuditRepository _repository;

    public StartAuditCommandHandler(IAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(StartAuditCommand request, CancellationToken cancellationToken)
    {
        var plan = await _repository.GetWithAuditsAsync(request.AuditPlanId, cancellationToken);
        if (plan is null)
            return Result.Failure(AuditErrors.NotFound);

        plan.StartAudit(request.AuditRecordId);
        await _repository.UpdateAsync(plan, cancellationToken);

        return Result.Success();
    }
}
