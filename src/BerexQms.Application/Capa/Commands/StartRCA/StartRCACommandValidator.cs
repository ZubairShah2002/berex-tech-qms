using FluentValidation;

namespace BerexQms.Application.Capa.Commands.StartRCA;

public sealed class StartRCACommandValidator : AbstractValidator<StartRCACommand>
{
    public StartRCACommandValidator()
    {
        RuleFor(x => x.CapaId)
            .NotEmpty().WithMessage("CAPA ID is required.");

        RuleFor(x => x.Methodology)
            .NotEmpty().WithMessage("RCA methodology is required.");

        RuleFor(x => x.AnalystId)
            .NotEmpty().WithMessage("Analyst ID is required.");
    }
}
