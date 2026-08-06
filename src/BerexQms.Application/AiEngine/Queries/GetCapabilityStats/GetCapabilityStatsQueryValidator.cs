using BerexQms.Domain.AiEngine.Enums;
using FluentValidation;

namespace BerexQms.Application.AiEngine.Queries.GetCapabilityStats;

public sealed class GetCapabilityStatsQueryValidator : AbstractValidator<GetCapabilityStatsQuery>
{
    public GetCapabilityStatsQueryValidator()
    {
        RuleFor(x => x.Capability)
            .NotEmpty().WithMessage("Capability is required.")
            .Must(v => Enum.TryParse<AiCapabilityType>(v, true, out _))
            .WithMessage("Invalid AI capability. Valid values: DefectPrediction, AnomalyDetection, " +
                         "RootCauseSuggestion, DocumentClassification, SupplierRiskScoring, " +
                         "InspectionOptimization.");

        RuleFor(x => x.Days)
            .InclusiveBetween(1, 365).WithMessage("Days must be between 1 and 365.");
    }
}
