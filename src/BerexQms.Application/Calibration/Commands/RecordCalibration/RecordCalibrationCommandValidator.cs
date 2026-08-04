using FluentValidation;

namespace BerexQms.Application.Calibration.Commands.RecordCalibration;

public sealed class RecordCalibrationCommandValidator : AbstractValidator<RecordCalibrationCommand>
{
    private static readonly string[] ValidResults = ["Pass", "PassWithAdjustment", "Fail", "Limited"];

    public RecordCalibrationCommandValidator()
    {
        RuleFor(x => x.EquipmentId).NotEmpty();
        RuleFor(x => x.CalibrationDate).NotEmpty();
        RuleFor(x => x.Result).NotEmpty().Must(r => ValidResults.Contains(r))
            .WithMessage("Result must be Pass, PassWithAdjustment, Fail, or Limited.");
        RuleFor(x => x.ProcedureRef).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.EnvironmentalConditions).MaximumLength(500);
    }
}
