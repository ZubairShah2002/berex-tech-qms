using FluentValidation;

namespace BerexQms.Application.Spc.Commands.AddDataPoint;

public sealed class AddDataPointCommandValidator : AbstractValidator<AddDataPointCommand>
{
    public AddDataPointCommandValidator()
    {
        RuleFor(x => x.ChartId)
            .NotEmpty().WithMessage("Chart ID is required.");

        RuleFor(x => x.SampleSize)
            .GreaterThanOrEqualTo(1).WithMessage("Sample size must be at least 1.");

        RuleFor(x => x.Timestamp)
            .NotEmpty().WithMessage("Timestamp is required.");
    }
}
