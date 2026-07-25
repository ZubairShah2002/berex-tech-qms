using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Inspection.DTOs;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Inspection.Entities;
using BerexQms.Domain.Inspection.Enums;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection.Commands.CreateSamplingPlan;

public sealed class CreateSamplingPlanCommandHandler
    : ICommandHandler<CreateSamplingPlanCommand, SamplingPlanDto>
{
    private readonly ISamplingPlanRepository _samplingPlanRepository;
    private readonly ITenantContext _tenantContext;

    public CreateSamplingPlanCommandHandler(
        ISamplingPlanRepository samplingPlanRepository,
        ITenantContext tenantContext)
    {
        _samplingPlanRepository = samplingPlanRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<SamplingPlanDto>> Handle(
        CreateSamplingPlanCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<InspectionType>(request.InspectionType, true, out var inspectionType))
            return InspectionErrors.InvalidInspectionType;

        if (!Enum.TryParse<SamplingLevel>(request.Level, true, out var level))
            return InspectionErrors.InvalidSamplingLevel;

        var plan = SamplingPlan.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.PartId,
            request.SupplierId,
            inspectionType,
            level,
            request.AqlValue,
            request.SampleSize,
            request.AcceptNumber,
            request.RejectNumber);

        await _samplingPlanRepository.AddAsync(plan, cancellationToken);

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
