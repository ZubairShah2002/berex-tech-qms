using BerexQms.Domain.AiEngine.Enums;
using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.RequestPrediction;

public sealed class RequestPredictionCommandValidator : AbstractValidator<RequestPredictionCommand>
{
    public RequestPredictionCommandValidator()
    {
        RuleFor(x => x.Capability)
            .NotEmpty().WithMessage("Capability is required.")
            .Must(v => Enum.TryParse<AiCapabilityType>(v, true, out _))
            .WithMessage("Invalid AI capability. Valid values: DefectPrediction, AnomalyDetection, " +
                         "RootCauseSuggestion, DocumentClassification, SupplierRiskScoring, " +
                         "InspectionOptimization.");
    }
}
