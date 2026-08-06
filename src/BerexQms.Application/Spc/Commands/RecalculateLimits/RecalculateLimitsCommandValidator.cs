using FluentValidation;

namespace BerexQms.Application.Spc.Commands.RecalculateLimits;

public sealed class RecalculateLimitsCommandValidator : AbstractValidator<RecalculateLimitsCommand>
{
    public RecalculateLimitsCommandValidator()
    {
        RuleFor(x => x.ChartId)
            .NotEmpty().WithMessage("Chart ID is required.");
    }
}
