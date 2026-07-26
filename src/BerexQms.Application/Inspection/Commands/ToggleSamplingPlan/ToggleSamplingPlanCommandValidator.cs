using FluentValidation;

namespace BerexQms.Application.Inspection.Commands.ToggleSamplingPlan;

public sealed class ToggleSamplingPlanCommandValidator : AbstractValidator<ToggleSamplingPlanCommand>
{
    public ToggleSamplingPlanCommandValidator()
    {
        RuleFor(x => x.SamplingPlanId)
            .NotEmpty().WithMessage("Sampling plan ID is required.");
    }
}
