using BerexQms.Domain.Inspection.Enums;
using FluentValidation;

namespace BerexQms.Application.Inspection.Commands.SetDisposition;

public sealed class SetDispositionCommandValidator : AbstractValidator<SetDispositionCommand>
{
    public SetDispositionCommandValidator()
    {
        RuleFor(x => x.InspectionId)
            .NotEmpty().WithMessage("Inspection ID is required.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Disposition type is required.")
            .Must(v => Enum.TryParse<DispositionType>(v, true, out _))
            .WithMessage("Invalid disposition type. Valid values: Accept, AcceptWithDeviation, Sort, Rework, ReturnToSupplier, Scrap.");

        RuleFor(x => x.Justification)
            .NotEmpty().WithMessage("Justification is required.")
            .MaximumLength(2000).WithMessage("Justification cannot exceed 2000 characters.");
    }
}
