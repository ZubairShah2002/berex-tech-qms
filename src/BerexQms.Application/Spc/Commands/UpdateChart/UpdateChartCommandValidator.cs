using FluentValidation;

namespace BerexQms.Application.Spc.Commands.UpdateChart;

public sealed class UpdateChartCommandValidator : AbstractValidator<UpdateChartCommand>
{
    public UpdateChartCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Chart ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Chart name is required.")
            .MaximumLength(200).WithMessage("Chart name cannot exceed 200 characters.");

        RuleFor(x => x.SubgroupSize)
            .GreaterThanOrEqualTo(1).WithMessage("Subgroup size must be at least 1.");

        RuleFor(x => x.LowerSpecLimit)
            .LessThan(x => x.UpperSpecLimit)
            .WithMessage("Lower spec limit must be less than the upper spec limit.")
            .When(x => x.LowerSpecLimit.HasValue && x.UpperSpecLimit.HasValue);
    }
}
