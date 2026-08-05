using BerexQms.Domain.Spc.Enums;
using FluentValidation;

namespace BerexQms.Application.Spc.Commands.CreateChart;

public sealed class CreateChartCommandValidator : AbstractValidator<CreateChartCommand>
{
    public CreateChartCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Chart code is required.")
            .MaximumLength(50).WithMessage("Chart code cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Chart name is required.")
            .MaximumLength(200).WithMessage("Chart name cannot exceed 200 characters.");

        RuleFor(x => x.ChartType)
            .NotEmpty().WithMessage("Chart type is required.")
            .Must(v => Enum.TryParse<ChartType>(v, true, out _))
            .WithMessage("Invalid chart type. Valid values: XBarR, XBarS, IndividualMovingRange, " +
                         "PChart, NpChart, CChart, UChart.");

        RuleFor(x => x.PartId)
            .NotEmpty().WithMessage("Part ID is required.");

        RuleFor(x => x.CharacteristicName)
            .NotEmpty().WithMessage("Characteristic name is required.")
            .MaximumLength(200).WithMessage("Characteristic name cannot exceed 200 characters.");

        RuleFor(x => x.SubgroupSize)
            .GreaterThanOrEqualTo(1).WithMessage("Subgroup size must be at least 1.");

        RuleFor(x => x.LowerSpecLimit)
            .LessThan(x => x.UpperSpecLimit)
            .WithMessage("Lower spec limit must be less than the upper spec limit.")
            .When(x => x.LowerSpecLimit.HasValue && x.UpperSpecLimit.HasValue);
    }
}
