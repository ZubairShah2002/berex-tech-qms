using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AuditManagement.DTOs;
using BerexQms.Domain.AuditManagement.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AuditManagement.Commands.AddChecklist;

internal sealed class AddChecklistCommandHandler : ICommandHandler<AddChecklistCommand, AuditChecklistDto>
{
    private readonly IAuditRepository _repository;

    public AddChecklistCommandHandler(IAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AuditChecklistDto>> Handle(AddChecklistCommand request, CancellationToken cancellationToken)
    {
        var plan = await _repository.GetWithAuditsAsync(request.AuditPlanId, cancellationToken);
        if (plan is null)
            return AuditErrors.NotFound;

        var checklist = plan.AddChecklist(
            request.AuditRecordId, request.Standard, request.ClauseReference,
            request.Requirement, request.IsCompliant, request.Evidence, request.Notes);

        await _repository.UpdateAsync(plan, cancellationToken);

        return new AuditChecklistDto(
            checklist.Id,
            checklist.AuditRecordId,
            checklist.Standard,
            checklist.ClauseReference,
            checklist.Requirement,
            checklist.IsCompliant,
            checklist.Evidence,
            checklist.Notes);
    }
}
