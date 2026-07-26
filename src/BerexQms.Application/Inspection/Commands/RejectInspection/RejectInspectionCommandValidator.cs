using FluentValidation;

namespace BerexQms.Application.Inspection.Commands.RejectInspection;

public sealed class RejectInspectionCommandValidator : AbstractValidator<RejectInspectionCommand>
{
    public RejectInspectionCommandValidator()
    {
        RuleFor(x => x.InspectionId)
            .NotEmpty().WithMessage("Inspection ID is required.");

        RuleFor(x => x.Notes)
            .NotEmpty().WithMessage("Rejection reason is required.")
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters.");
    }
}
