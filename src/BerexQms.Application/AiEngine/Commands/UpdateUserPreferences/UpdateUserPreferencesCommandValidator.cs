using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.UpdateUserPreferences;

public sealed class UpdateUserPreferencesCommandValidator
    : AbstractValidator<UpdateUserPreferencesCommand>
{
    private static readonly HashSet<string> ValidProviders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Automatic", "Local", "Claude", "OpenAi",
    };

    private static readonly HashSet<string> ValidTaskTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "DocumentAnalysis", "QualityAnalysis", "RecommendationGeneration",
        "Summarization", "RiskAnalysis", "CAPAAnalysis", "SupplierAnalysis",
        "AuditAnalysis", "DefectTrendAnalysis", "StructuredDataExtraction",
    };

    public UpdateUserPreferencesCommandValidator()
    {
        RuleFor(x => x.PreferredProvider)
            .Must(p => string.IsNullOrEmpty(p) || ValidProviders.Contains(p))
            .WithMessage("PreferredProvider must be 'Automatic', 'Local', 'Claude', or 'OpenAi'.");

        RuleForEach(x => x.EnabledTaskTypes)
            .Must(t => ValidTaskTypes.Contains(t))
            .WithMessage("'{PropertyValue}' is not a valid AI task type.");

        RuleFor(x => x.PreferredLanguage)
            .MaximumLength(10)
            .When(x => x.PreferredLanguage != null);
    }
}
