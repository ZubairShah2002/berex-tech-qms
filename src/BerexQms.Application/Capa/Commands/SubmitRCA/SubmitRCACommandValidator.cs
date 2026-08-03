using FluentValidation;

namespace BerexQms.Application.Capa.Commands.SubmitRCA;

public sealed class SubmitRCACommandValidator : AbstractValidator<SubmitRCACommand>
{
    public SubmitRCACommandValidator()
    {
        RuleFor(x => x.CapaId)
            .NotEmpty().WithMessage("CAPA ID is required.");

        RuleFor(x => x.RootCause)
            .NotEmpty().WithMessage("Root cause is required.")
            .MaximumLength(4000).WithMessage("Root cause must not exceed 4000 characters.");

        RuleFor(x => x.AnalysisDetails)
            .MaximumLength(4000).WithMessage("Analysis details must not exceed 4000 characters.")
            .When(x => x.AnalysisDetails is not null);

        RuleFor(x => x.ContributingFactors)
            .MaximumLength(4000).WithMessage("Contributing factors must not exceed 4000 characters.")
            .When(x => x.ContributingFactors is not null);
    }
}
