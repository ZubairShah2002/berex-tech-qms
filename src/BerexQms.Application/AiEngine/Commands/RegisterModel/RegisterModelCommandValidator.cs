using BerexQms.Domain.AiEngine.Enums;
using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.RegisterModel;

public sealed class RegisterModelCommandValidator : AbstractValidator<RegisterModelCommand>
{
    public RegisterModelCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Model name is required.")
            .MaximumLength(200).WithMessage("Model name cannot exceed 200 characters.");

        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Model version is required.")
            .MaximumLength(50).WithMessage("Model version cannot exceed 50 characters.");

        RuleFor(x => x.Capability)
            .NotEmpty().WithMessage("Capability is required.")
            .Must(v => Enum.TryParse<AiCapabilityType>(v, true, out _))
            .WithMessage("Invalid AI capability. Valid values: DefectPrediction, AnomalyDetection, " +
                         "RootCauseSuggestion, DocumentClassification, SupplierRiskScoring, " +
                         "InspectionOptimization.");
    }
}
