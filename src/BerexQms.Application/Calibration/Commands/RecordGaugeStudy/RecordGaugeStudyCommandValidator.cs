using FluentValidation;

namespace BerexQms.Application.Calibration.Commands.RecordGaugeStudy;

public sealed class RecordGaugeStudyCommandValidator : AbstractValidator<RecordGaugeStudyCommand>
{
    public RecordGaugeStudyCommandValidator()
    {
        RuleFor(x => x.EquipmentId).NotEmpty();
        RuleFor(x => x.StudyDate).NotEmpty();
        RuleFor(x => x.TotalGRRPct).InclusiveBetween(0, 100);
        RuleFor(x => x.RepeatabilityPct).InclusiveBetween(0, 100);
        RuleFor(x => x.ReproducibilityPct).InclusiveBetween(0, 100);
        RuleFor(x => x.PartVariationPct).InclusiveBetween(0, 100).When(x => x.PartVariationPct.HasValue);
        RuleFor(x => x.Ndc).GreaterThanOrEqualTo(0).When(x => x.Ndc.HasValue);
    }
}
