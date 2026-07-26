using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Inspection.DTOs;
using BerexQms.Domain.Inspection.Entities;
using BerexQms.Domain.Inspection.Enums;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection.Queries.ListSamplingPlans;

public sealed class ListSamplingPlansQueryHandler
    : IQueryHandler<ListSamplingPlansQuery, PagedResult<SamplingPlanDto>>
{
    private readonly ISamplingPlanRepository _samplingPlanRepository;

    public ListSamplingPlansQueryHandler(ISamplingPlanRepository samplingPlanRepository)
    {
        _samplingPlanRepository = samplingPlanRepository;
    }

    public async Task<Result<PagedResult<SamplingPlanDto>>> Handle(
        ListSamplingPlansQuery request, CancellationToken cancellationToken)
    {
        var spec = new SamplingPlanListSpecification(
            request.PartId, request.InspectionType, request.IsActive,
            request.Page, request.PageSize);

        var plans = await _samplingPlanRepository.ListAsync(spec, cancellationToken);

        var countSpec = new SamplingPlanCountSpecification(
            request.PartId, request.InspectionType, request.IsActive);
        var totalCount = await _samplingPlanRepository.CountAsync(countSpec, cancellationToken);

        var dtos = plans.Select(p => new SamplingPlanDto(
            p.Id,
            p.PartId,
            p.SupplierId,
            p.InspectionType.ToString(),
            p.Level.ToString(),
            p.AqlValue,
            p.SampleSize,
            p.AcceptNumber,
            p.RejectNumber,
            p.IsActive,
            p.CreatedAt)).ToList();

        return new PagedResult<SamplingPlanDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    private sealed class SamplingPlanListSpecification : Specification<SamplingPlan>
    {
        public SamplingPlanListSpecification(
            Guid? partId, string? inspectionType, bool? isActive,
            int page, int pageSize)
        {
            ApplyFilters(partId, inspectionType, isActive);
            ApplyOrderByDescending(p => p.CreatedAt);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }

        private void ApplyFilters(Guid? partId, string? inspectionType, bool? isActive)
        {
            var hasPart = partId.HasValue;
            var hasType = !string.IsNullOrWhiteSpace(inspectionType)
                          && Enum.TryParse<InspectionType>(inspectionType, true, out _);
            var hasActive = isActive.HasValue;

            if (!hasPart && !hasType && !hasActive)
                return;

            var pid = partId ?? Guid.Empty;
            var parsedType = hasType ? Enum.Parse<InspectionType>(inspectionType!, true) : default;
            var active = isActive ?? false;

            ApplyCriteria(p =>
                (!hasPart || p.PartId == pid) &&
                (!hasType || p.InspectionType == parsedType) &&
                (!hasActive || p.IsActive == active));
        }
    }

    private sealed class SamplingPlanCountSpecification : Specification<SamplingPlan>
    {
        public SamplingPlanCountSpecification(
            Guid? partId, string? inspectionType, bool? isActive)
        {
            var hasPart = partId.HasValue;
            var hasType = !string.IsNullOrWhiteSpace(inspectionType)
                          && Enum.TryParse<InspectionType>(inspectionType, true, out _);
            var hasActive = isActive.HasValue;

            if (!hasPart && !hasType && !hasActive)
                return;

            var pid = partId ?? Guid.Empty;
            var parsedType = hasType ? Enum.Parse<InspectionType>(inspectionType!, true) : default;
            var active = isActive ?? false;

            ApplyCriteria(p =>
                (!hasPart || p.PartId == pid) &&
                (!hasType || p.InspectionType == parsedType) &&
                (!hasActive || p.IsActive == active));
        }
    }
}
