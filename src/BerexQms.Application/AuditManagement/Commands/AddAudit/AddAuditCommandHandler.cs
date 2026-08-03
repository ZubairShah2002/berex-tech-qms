using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AuditManagement.DTOs;
using BerexQms.Domain.AuditManagement.Enums;
using BerexQms.Domain.AuditManagement.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AuditManagement.Commands.AddAudit;

internal sealed class AddAuditCommandHandler : ICommandHandler<AddAuditCommand, AuditRecordDto>
{
    private readonly IAuditRepository _repository;

    public AddAuditCommandHandler(IAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AuditRecordDto>> Handle(AddAuditCommand request, CancellationToken cancellationToken)
    {
        var plan = await _repository.GetWithAuditsAsync(request.AuditPlanId, cancellationToken);
        if (plan is null)
            return AuditErrors.NotFound;

        if (!Enum.TryParse<AuditType>(request.AuditType, true, out var auditType))
            return Error.Validation("Audit.InvalidType", $"Invalid audit type: {request.AuditType}.");

        var audit = plan.AddAudit(
            request.AuditNumber, auditType, request.LeadAuditorId,
            request.AuditeeArea, request.ScheduledDate);

        await _repository.UpdateAsync(plan, cancellationToken);

        return new AuditRecordDto(
            audit.Id,
            audit.AuditNumber,
            audit.AuditType.ToString(),
            audit.Status.ToString(),
            audit.LeadAuditorId,
            audit.AuditeeArea,
            audit.ScheduledDate,
            audit.StartedAt,
            audit.CompletedAt,
            audit.Findings.Count,
            audit.Report is not null);
    }
}
