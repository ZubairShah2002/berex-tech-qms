using FluentValidation;

namespace BerexQms.Application.NonConformance.Commands.ReopenNonConformance;

public sealed class ReopenNonConformanceCommandValidator : AbstractValidator<ReopenNonConformanceCommand>
{
    public ReopenNonConformanceCommandValidator()
    {
        RuleFor(x => x.NonConformanceId)
            .NotEmpty().WithMessage("Non-conformance ID is required.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason for reopening is required.")
            .MaximumLength(4000).WithMessage("Reason cannot exceed 4000 characters.");
    }
}
