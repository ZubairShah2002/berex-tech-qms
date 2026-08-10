using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.ExecuteAiAnalysis;

public sealed class ExecuteAiAnalysisCommandValidator : AbstractValidator<ExecuteAiAnalysisCommand>
{
    private static readonly string[] ValidTaskTypes =
    [
        "DocumentAnalysis", "QualityAnalysis", "RecommendationGeneration",
        "Summarization", "RiskAnalysis", "CAPAAnalysis",
        "SupplierAnalysis", "AuditAnalysis", "DefectTrendAnalysis",
        "StructuredDataExtraction",
    ];

    public ExecuteAiAnalysisCommandValidator()
    {
        RuleFor(x => x.TaskType)
            .NotEmpty()
            .Must(t => ValidTaskTypes.Contains(t))
            .WithMessage("Invalid AI task type.");

        RuleFor(x => x.MaxContextDocuments)
            .InclusiveBetween(1, 50);
    }
}
