using FluentValidation;

namespace BerexQms.Application.Inspection.Commands.CancelInspection;

public sealed class CancelInspectionCommandValidator : AbstractValidator<CancelInspectionCommand>
{
    public CancelInspectionCommandValidator()
    {
        RuleFor(x => x.InspectionId)
            .NotEmpty().WithMessage("Inspection ID is required.");
    }
}
