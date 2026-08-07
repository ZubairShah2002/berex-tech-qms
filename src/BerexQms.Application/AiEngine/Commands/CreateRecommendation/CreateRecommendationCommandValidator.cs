using BerexQms.Domain.AiEngine.Enums;
using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.CreateRecommendation;

public sealed class CreateRecommendationCommandValidator
    : AbstractValidator<CreateRecommendationCommand>
{
    public CreateRecommendationCommandValidator()
    {
        RuleFor(x => x.RecommendationType)
            .NotEmpty().WithMessage("Recommendation type is required.")
            .Must(v => Enum.TryParse<AiRecommendationType>(v, true, out _))
            .WithMessage("Invalid recommendation type. Valid values: DefectTrend, SupplierRisk, " +
                         "ProcessRisk, DocumentGap, AuditRisk, CAPARecommendation.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(500).WithMessage("Title cannot exceed 500 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(5000).WithMessage("Description cannot exceed 5,000 characters.");

        RuleFor(x => x.Severity)
            .NotEmpty().WithMessage("Severity is required.")
            .Must(v => Enum.TryParse<AiSeverity>(v, true, out _))
            .WithMessage("Invalid severity. Valid values: Low, Medium, High, Critical.");

        RuleFor(x => x.RelatedModule)
            .NotEmpty().WithMessage("Related module is required.")
            .MaximumLength(100).WithMessage("Related module cannot exceed 100 characters.");

        RuleFor(x => x.RelatedEntityId)
            .MaximumLength(200).WithMessage("Related entity ID cannot exceed 200 characters.");

        RuleFor(x => x.ConfidenceScore)
            .InclusiveBetween(0m, 1m).WithMessage("Confidence score must be between 0 and 1.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required.")
            .MaximumLength(5000).WithMessage("Reason cannot exceed 5,000 characters.");

        RuleFor(x => x.SupportingData)
            .MaximumLength(50_000).WithMessage("Supporting data cannot exceed 50,000 characters.");

        RuleFor(x => x.RecommendedAction)
            .MaximumLength(2000).WithMessage("Recommended action cannot exceed 2,000 characters.");

        RuleFor(x => x.SourceContextIds)
            .MaximumLength(2000).WithMessage("Source context IDs cannot exceed 2,000 characters.");
    }
}
