using FluentValidation;

namespace BerexQms.Application.Inspection.Commands.StartInspection;

public sealed class StartInspectionCommandValidator : AbstractValidator<StartInspectionCommand>
{
    public StartInspectionCommandValidator()
    {
        RuleFor(x => x.InspectionId)
            .NotEmpty().WithMessage("Inspection ID is required.");
    }
}
