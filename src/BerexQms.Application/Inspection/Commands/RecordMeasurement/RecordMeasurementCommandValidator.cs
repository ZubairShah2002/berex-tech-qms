using BerexQms.Domain.Inspection.Enums;
using FluentValidation;

namespace BerexQms.Application.Inspection.Commands.RecordMeasurement;

public sealed class RecordMeasurementCommandValidator : AbstractValidator<RecordMeasurementCommand>
{
    public RecordMeasurementCommandValidator()
    {
        RuleFor(x => x.InspectionId)
            .NotEmpty().WithMessage("Inspection ID is required.");

        RuleFor(x => x.CharacteristicName)
            .NotEmpty().WithMessage("Characteristic name is required.")
            .MaximumLength(200).WithMessage("Characteristic name cannot exceed 200 characters.");

        RuleFor(x => x.Result)
            .NotEmpty().WithMessage("Measurement result is required.")
            .Must(v => Enum.TryParse<MeasurementResult>(v, true, out _))
            .WithMessage("Invalid measurement result. Valid values: Pass, Fail, NotApplicable.");

        RuleFor(x => x)
            .Must(x => x.MeasuredValue.HasValue || !string.IsNullOrWhiteSpace(x.TextValue))
            .WithMessage("Either a measured value or text value is required.")
            .When(x => x.Result is "Pass" or "Fail");

        RuleFor(x => x.Unit)
            .MaximumLength(20).WithMessage("Unit cannot exceed 20 characters.")
            .When(x => x.Unit is not null);

        RuleFor(x => x.TextValue)
            .MaximumLength(500).WithMessage("Text value cannot exceed 500 characters.")
            .When(x => x.TextValue is not null);

        RuleFor(x => x.OperatorId)
            .MaximumLength(100).WithMessage("Operator ID cannot exceed 100 characters.")
            .When(x => x.OperatorId is not null);
    }
}
