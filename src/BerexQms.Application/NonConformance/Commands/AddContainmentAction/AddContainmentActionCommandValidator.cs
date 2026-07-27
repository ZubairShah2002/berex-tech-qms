using FluentValidation;

namespace BerexQms.Application.NonConformance.Commands.AddContainmentAction;

public sealed class AddContainmentActionCommandValidator : AbstractValidator<AddContainmentActionCommand>
{
    public AddContainmentActionCommandValidator()
    {
        RuleFor(x => x.NonConformanceId)
            .NotEmpty().WithMessage("Non-conformance ID is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");
    }
}
