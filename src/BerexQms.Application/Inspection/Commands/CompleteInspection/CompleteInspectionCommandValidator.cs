using FluentValidation;

namespace BerexQms.Application.Inspection.Commands.CompleteInspection;

public sealed class CompleteInspectionCommandValidator : AbstractValidator<CompleteInspectionCommand>
{
    public CompleteInspectionCommandValidator()
    {
        RuleFor(x => x.InspectionId)
            .NotEmpty().WithMessage("Inspection ID is required.");
    }
}
