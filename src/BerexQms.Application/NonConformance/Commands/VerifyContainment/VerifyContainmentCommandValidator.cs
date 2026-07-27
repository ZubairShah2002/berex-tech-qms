using FluentValidation;

namespace BerexQms.Application.NonConformance.Commands.VerifyContainment;

public sealed class VerifyContainmentCommandValidator : AbstractValidator<VerifyContainmentCommand>
{
    public VerifyContainmentCommandValidator()
    {
        RuleFor(x => x.NonConformanceId)
            .NotEmpty().WithMessage("Non-conformance ID is required.");

        RuleFor(x => x.ContainmentActionId)
            .NotEmpty().WithMessage("Containment action ID is required.");
    }
}
