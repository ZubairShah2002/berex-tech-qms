using BerexQms.Domain.Inspection.Enums;
using FluentValidation;

namespace BerexQms.Application.Inspection.Commands.UpdateSamplingPlan;

public sealed class UpdateSamplingPlanCommandValidator : AbstractValidator<UpdateSamplingPlanCommand>
{
    public UpdateSamplingPlanCommandValidator()
    {
        RuleFor(x => x.SamplingPlanId)
            .NotEmpty().WithMessage("Sampling plan ID is required.");

        RuleFor(x => x.Level)
            .NotEmpty().WithMessage("Sampling level is required.")
            .Must(v => Enum.TryParse<SamplingLevel>(v, true, out _))
            .WithMessage("Invalid sampling level. Valid values: Normal, Tightened, Reduced.");

        RuleFor(x => x.AqlValue)
            .GreaterThan(0).WithMessage("AQL value must be greater than zero.");

        RuleFor(x => x.SampleSize)
            .GreaterThan(0).WithMessage("Sample size must be greater than zero.");

        RuleFor(x => x.AcceptNumber)
            .GreaterThanOrEqualTo(0).WithMessage("Accept number cannot be negative.");

        RuleFor(x => x.RejectNumber)
            .GreaterThan(0).WithMessage("Reject number must be greater than zero.")
            .GreaterThan(x => x.AcceptNumber).WithMessage("Reject number must be greater than accept number.");
    }
}
