using FluentValidation;

namespace BerexQms.Application.Spc.Commands.DeactivateChart;

public sealed class DeactivateChartCommandValidator : AbstractValidator<DeactivateChartCommand>
{
    public DeactivateChartCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Chart ID is required.");
    }
}
