using FluentValidation;

namespace BerexQms.Application.NonConformance.Commands.AssignInvestigator;

public sealed class AssignInvestigatorCommandValidator : AbstractValidator<AssignInvestigatorCommand>
{
    public AssignInvestigatorCommandValidator()
    {
        RuleFor(x => x.NonConformanceId)
            .NotEmpty().WithMessage("Non-conformance ID is required.");

        RuleFor(x => x.InvestigatorId)
            .NotEmpty().WithMessage("Investigator ID is required.")
            .MaximumLength(100).WithMessage("Investigator ID cannot exceed 100 characters.");
    }
}
