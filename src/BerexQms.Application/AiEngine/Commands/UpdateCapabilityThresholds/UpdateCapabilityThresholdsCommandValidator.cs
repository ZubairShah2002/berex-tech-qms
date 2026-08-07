using BerexQms.Domain.AiEngine.Enums;
using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.UpdateCapabilityThresholds;

public sealed class UpdateCapabilityThresholdsCommandValidator
    : AbstractValidator<UpdateCapabilityThresholdsCommand>
{
    public UpdateCapabilityThresholdsCommandValidator()
    {
        RuleFor(x => x.Capability)
            .NotEmpty().WithMessage("Capability is required.")
            .Must(v => Enum.TryParse<AiCapabilityType>(v, true, out _))
            .WithMessage("Invalid AI capability. Valid values: DefectPrediction, AnomalyDetection, " +
                         "RootCauseSuggestion, DocumentClassification, SupplierRiskScoring, " +
                         "InspectionOptimization.");

        RuleFor(x => x.LowThreshold)
            .NotEmpty().WithMessage("Low confidence threshold is required.")
            .InclusiveBetween(0.20m, 1m)
            .WithMessage("Low confidence threshold must be between 0.20 and 1.");

        RuleFor(x => x.ModerateThreshold)
            .NotEmpty().WithMessage("Moderate confidence threshold is required.")
            .InclusiveBetween(0m, 1m)
            .WithMessage("Moderate confidence threshold must be between 0 and 1.");

        RuleFor(x => x.HighThreshold)
            .NotEmpty().WithMessage("High confidence threshold is required.")
            .InclusiveBetween(0m, 1m)
            .WithMessage("High confidence threshold must be between 0 and 1.");

        RuleFor(x => x)
            .Must(x => x.LowThreshold < x.ModerateThreshold && x.ModerateThreshold < x.HighThreshold)
            .WithMessage("Thresholds must satisfy low < moderate < high.")
            .WithName("Thresholds");
    }
}
