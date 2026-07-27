using FluentValidation;

namespace BerexQms.Application.NonConformance.Commands.CloseAsDuplicate;

public sealed class CloseAsDuplicateCommandValidator : AbstractValidator<CloseAsDuplicateCommand>
{
    public CloseAsDuplicateCommandValidator()
    {
        RuleFor(x => x.NonConformanceId)
            .NotEmpty().WithMessage("Non-conformance ID is required.");

        RuleFor(x => x.Notes)
            .NotEmpty().WithMessage("Notes are required when closing as duplicate.")
            .MaximumLength(4000).WithMessage("Notes cannot exceed 4000 characters.");
    }
}
