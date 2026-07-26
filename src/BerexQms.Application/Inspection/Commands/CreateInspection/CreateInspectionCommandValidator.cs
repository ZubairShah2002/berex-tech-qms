using BerexQms.Domain.Inspection.Enums;
using FluentValidation;

namespace BerexQms.Application.Inspection.Commands.CreateInspection;

public sealed class CreateInspectionCommandValidator : AbstractValidator<CreateInspectionCommand>
{
    public CreateInspectionCommandValidator()
    {
        RuleFor(x => x.InspectionNumber)
            .NotEmpty().WithMessage("Inspection number is required.")
            .MaximumLength(50).WithMessage("Inspection number cannot exceed 50 characters.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Inspection type is required.")
            .Must(v => Enum.TryParse<InspectionType>(v, true, out _))
            .WithMessage("Invalid inspection type. Valid values: IQC, IPQC, OQC.");

        RuleFor(x => x.PartId)
            .NotEmpty().WithMessage("Part ID is required.");

        RuleFor(x => x.InspectorId)
            .NotEmpty().WithMessage("Inspector ID is required.");

        RuleFor(x => x.LotSize)
            .GreaterThan(0).WithMessage("Lot size must be greater than zero.")
            .When(x => x.LotSize.HasValue);

        RuleFor(x => x.SampleSize)
            .GreaterThan(0).WithMessage("Sample size must be greater than zero.")
            .When(x => x.SampleSize.HasValue);

        RuleFor(x => x.SampleSize)
            .LessThanOrEqualTo(x => x.LotSize)
            .WithMessage("Sample size cannot exceed lot size.")
            .When(x => x.SampleSize.HasValue && x.LotSize.HasValue);

        RuleFor(x => x.LotNumber)
            .MaximumLength(100).WithMessage("Lot number cannot exceed 100 characters.")
            .When(x => x.LotNumber is not null);
    }
}
