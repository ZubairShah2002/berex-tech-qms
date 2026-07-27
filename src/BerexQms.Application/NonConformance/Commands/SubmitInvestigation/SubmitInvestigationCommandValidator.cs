using FluentValidation;

namespace BerexQms.Application.NonConformance.Commands.SubmitInvestigation;

public sealed class SubmitInvestigationCommandValidator : AbstractValidator<SubmitInvestigationCommand>
{
    public SubmitInvestigationCommandValidator()
    {
        RuleFor(x => x.NonConformanceId)
            .NotEmpty().WithMessage("Non-conformance ID is required.");

        RuleFor(x => x.RootCause)
            .NotEmpty().WithMessage("Root cause is required.")
            .MaximumLength(4000).WithMessage("Root cause cannot exceed 4000 characters.");

        RuleFor(x => x.Findings)
            .NotEmpty().WithMessage("Findings are required.")
            .MaximumLength(4000).WithMessage("Findings cannot exceed 4000 characters.");

        RuleFor(x => x.Methodology)
            .MaximumLength(200).WithMessage("Methodology cannot exceed 200 characters.")
            .When(x => x.Methodology is not null);
    }
}
