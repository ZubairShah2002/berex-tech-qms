using BerexQms.Domain.Inspection.Enums;
using FluentValidation;

namespace BerexQms.Application.Inspection.Commands.CreateSamplingPlan;

public sealed class CreateSamplingPlanCommandValidator : AbstractValidator<CreateSamplingPlanCommand>
{
    public CreateSamplingPlanCommandValidator()
    {
        RuleFor(x => x.PartId)
            .NotEmpty().WithMessage("Part ID is required.");

        RuleFor(x => x.InspectionType)
            .NotEmpty().WithMessage("Inspection type is required.")
            .Must(v => Enum.TryParse<InspectionType>(v, true, out _))
            .WithMessage("Invalid inspection type. Valid values: IQC, IPQC, OQC.");

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
