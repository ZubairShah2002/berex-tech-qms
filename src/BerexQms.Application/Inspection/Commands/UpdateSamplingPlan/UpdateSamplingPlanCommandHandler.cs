using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.Inspection.Enums;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Inspection.Commands.UpdateSamplingPlan;

public sealed class UpdateSamplingPlanCommandHandler : ICommandHandler<UpdateSamplingPlanCommand>
{
    private readonly ISamplingPlanRepository _samplingPlanRepository;

    public UpdateSamplingPlanCommandHandler(ISamplingPlanRepository samplingPlanRepository)
    {
        _samplingPlanRepository = samplingPlanRepository;
    }

    public async Task<Result> Handle(UpdateSamplingPlanCommand request, CancellationToken cancellationToken)
    {
        var plan = await _samplingPlanRepository.GetByIdAsync(request.SamplingPlanId, cancellationToken);
        if (plan is null)
            return Result.Failure(InspectionErrors.SamplingPlanNotFound);

        if (!Enum.TryParse<SamplingLevel>(request.Level, true, out var level))
            return Result.Failure(InspectionErrors.InvalidSamplingLevel);

        plan.Update(level, request.AqlValue, request.SampleSize, request.AcceptNumber, request.RejectNumber);
        await _samplingPlanRepository.UpdateAsync(plan, cancellationToken);

        return Result.Success();
    }
}
