using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection.Commands.ToggleSamplingPlan;

public sealed class ToggleSamplingPlanCommandHandler : ICommandHandler<ToggleSamplingPlanCommand>
{
    private readonly ISamplingPlanRepository _samplingPlanRepository;

    public ToggleSamplingPlanCommandHandler(ISamplingPlanRepository samplingPlanRepository)
    {
        _samplingPlanRepository = samplingPlanRepository;
    }

    public async Task<Result> Handle(ToggleSamplingPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _samplingPlanRepository.GetByIdAsync(request.SamplingPlanId, cancellationToken);
        if (plan is null)
            return Result.Failure(InspectionErrors.SamplingPlanNotFound);

        if (request.Activate)
            plan.Activate();
        else
            plan.Deactivate();

        await _samplingPlanRepository.UpdateAsync(plan, cancellationToken);

        return Result.Success();
    }
}
