using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Inspection.DTOs;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection.Queries.GetSamplingPlanById;

public sealed class GetSamplingPlanByIdQueryHandler
    : IQueryHandler<GetSamplingPlanByIdQuery, SamplingPlanDto>
{
    private readonly ISamplingPlanRepository _samplingPlanRepository;

    public GetSamplingPlanByIdQueryHandler(ISamplingPlanRepository samplingPlanRepository)
    {
        _samplingPlanRepository = samplingPlanRepository;
    }

    public async Task<Result<SamplingPlanDto>> Handle(
        GetSamplingPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await _samplingPlanRepository.GetByIdAsync(
            request.SamplingPlanId, cancellationToken);
        if (plan is null)
            return InspectionErrors.SamplingPlanNotFound;

        return new SamplingPlanDto(
            plan.Id,
            plan.PartId,
            plan.SupplierId,
            plan.InspectionType.ToString(),
            plan.Level.ToString(),
            plan.AqlValue,
            plan.SampleSize,
            plan.AcceptNumber,
            plan.RejectNumber,
            plan.IsActive,
            plan.CreatedAt);
    }
}
