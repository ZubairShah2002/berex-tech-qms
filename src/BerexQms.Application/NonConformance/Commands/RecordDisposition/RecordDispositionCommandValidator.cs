using BerexQms.Domain.NonConformance.Enums;
using FluentValidation;

namespace BerexQms.Application.NonConformance.Commands.RecordDisposition;

public sealed class RecordDispositionCommandValidator : AbstractValidator<RecordDispositionCommand>
{
    public RecordDispositionCommandValidator()
    {
        RuleFor(x => x.NonConformanceId)
            .NotEmpty().WithMessage("Non-conformance ID is required.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Disposition type is required.")
            .Must(v => Enum.TryParse<NCDispositionType>(v, true, out _))
            .WithMessage("Invalid disposition type. Valid values: UseAsIs, Rework, Scrap, ReturnToSupplier.");

        RuleFor(x => x.Justification)
            .NotEmpty().WithMessage("Justification is required.")
            .MaximumLength(4000).WithMessage("Justification cannot exceed 4000 characters.");
    }
}
