using FluentValidation;

namespace BerexQms.Application.Inspection.Commands.ApproveInspection;

public sealed class ApproveInspectionCommandValidator : AbstractValidator<ApproveInspectionCommand>
{
    public ApproveInspectionCommandValidator()
    {
        RuleFor(x => x.InspectionId)
            .NotEmpty().WithMessage("Inspection ID is required.");
    }
}
