using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.AuditManagement.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AuditManagement.Commands.CompleteAudit;

internal sealed class CompleteAuditCommandHandler : ICommandHandler<CompleteAuditCommand>
{
    private readonly IAuditRepository _repository;

    public CompleteAuditCommandHandler(IAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(CompleteAuditCommand request, CancellationToken cancellationToken)
    {
        var plan = await _repository.GetWithAuditsAsync(request.AuditPlanId, cancellationToken);
        if (plan is null)
            return Result.Failure(AuditErrors.NotFound);

        plan.CompleteAudit(request.AuditRecordId, request.Summary, request.Recommendations, request.AuditorNotes);
        await _repository.UpdateAsync(plan, cancellationToken);

        return Result.Success();
    }
}
