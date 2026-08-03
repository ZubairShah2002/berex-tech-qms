using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AuditManagement.DTOs;
using BerexQms.Domain.AuditManagement.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AuditManagement.Queries.GetAuditPlanById;

internal sealed class GetAuditPlanByIdQueryHandler : IQueryHandler<GetAuditPlanByIdQuery, AuditPlanDetailDto>
{
    private readonly IAuditRepository _repository;

    public GetAuditPlanByIdQueryHandler(IAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AuditPlanDetailDto>> Handle(
        GetAuditPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await _repository.GetFullDetailAsync(request.AuditPlanId, cancellationToken);
        if (plan is null)
            return AuditErrors.NotFound;

        var audits = plan.Audits.Select(a => new AuditRecordDto(
            a.Id,
            a.AuditNumber,
            a.AuditType.ToString(),
            a.Status.ToString(),
            a.LeadAuditorId,
            a.AuditeeArea,
            a.ScheduledDate,
            a.StartedAt,
            a.CompletedAt,
            a.Findings.Count,
            a.Report is not null)).ToList();

        return new AuditPlanDetailDto(
            plan.Id,
            plan.PlanName,
            plan.Year,
            plan.Description,
            plan.Scope,
            plan.IsActive,
            audits,
            plan.CreatedAt,
            plan.CreatedBy,
            plan.ModifiedAt);
    }
}
