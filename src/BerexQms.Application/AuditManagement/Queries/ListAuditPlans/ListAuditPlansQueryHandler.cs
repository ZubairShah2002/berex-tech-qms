using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AuditManagement.DTOs;
using BerexQms.Domain.AuditManagement.Entities;
using BerexQms.Domain.AuditManagement.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AuditManagement.Queries.ListAuditPlans;

internal sealed class ListAuditPlansQueryHandler
    : IQueryHandler<ListAuditPlansQuery, PagedResult<AuditPlanDto>>
{
    private readonly IAuditRepository _repository;

    public ListAuditPlansQueryHandler(IAuditRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<AuditPlanDto>>> Handle(
        ListAuditPlansQuery request, CancellationToken cancellationToken)
    {
        var spec = new AuditPlanListSpecification(
            request.SearchTerm, request.Year, request.IsActive, request.Page, request.PageSize);

        var plans = await _repository.ListAsync(spec, cancellationToken);

        var countSpec = new AuditPlanCountSpecification(
            request.SearchTerm, request.Year, request.IsActive);
        var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

        var dtos = plans.Select(p => new AuditPlanDto(
            p.Id,
            p.PlanName,
            p.Year,
            p.Description,
            p.Scope,
            p.IsActive,
            p.Audits.Count,
            p.CreatedAt)).ToList();

        return new PagedResult<AuditPlanDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    private sealed class AuditPlanListSpecification : Specification<AuditPlan>
    {
        public AuditPlanListSpecification(
            string? searchTerm, int? year, bool? isActive, int page, int pageSize)
        {
            ApplyFilters(searchTerm, year, isActive);
            ApplyOrderByDescending(p => p.CreatedAt);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }

        private void ApplyFilters(string? searchTerm, int? year, bool? isActive)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasYear = year.HasValue;
            var hasActive = isActive.HasValue;

            if (!hasSearch && !hasYear && !hasActive)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;

            ApplyCriteria(p =>
                (!hasSearch || p.PlanName.ToUpper().Contains(term)) &&
                (!hasYear || p.Year == year!.Value) &&
                (!hasActive || p.IsActive == isActive!.Value));
        }
    }

    private sealed class AuditPlanCountSpecification : Specification<AuditPlan>
    {
        public AuditPlanCountSpecification(string? searchTerm, int? year, bool? isActive)
        {
            var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
            var hasYear = year.HasValue;
            var hasActive = isActive.HasValue;

            if (!hasSearch && !hasYear && !hasActive)
                return;

            var term = searchTerm?.ToUpperInvariant() ?? string.Empty;

            ApplyCriteria(p =>
                (!hasSearch || p.PlanName.ToUpper().Contains(term)) &&
                (!hasYear || p.Year == year!.Value) &&
                (!hasActive || p.IsActive == isActive!.Value));
        }
    }
}
