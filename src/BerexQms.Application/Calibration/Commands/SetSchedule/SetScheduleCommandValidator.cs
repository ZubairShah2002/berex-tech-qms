using FluentValidation;

namespace BerexQms.Application.Calibration.Commands.SetSchedule;

public sealed class SetScheduleCommandValidator : AbstractValidator<SetScheduleCommand>
{
    public SetScheduleCommandValidator()
    {
        RuleFor(x => x.EquipmentId).NotEmpty();
        RuleFor(x => x.IntervalDays).GreaterThan(0);
        RuleFor(x => x.LeadTimeDays).GreaterThanOrEqualTo(0);
        RuleFor(x => x.LabType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ProcedureRef).MaximumLength(200);
    }
}
