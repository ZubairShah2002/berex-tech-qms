using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.UpdateGovernancePolicy;

public sealed class UpdateGovernancePolicyCommandValidator
    : AbstractValidator<UpdateGovernancePolicyCommand>
{
    private static readonly HashSet<string> ValidProviders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Local", "Claude", "OpenAi",
    };

    private static readonly HashSet<string> ValidTaskTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "DocumentAnalysis", "QualityAnalysis", "RecommendationGeneration",
        "Summarization", "RiskAnalysis", "CAPAAnalysis", "SupplierAnalysis",
        "AuditAnalysis", "DefectTrendAnalysis", "StructuredDataExtraction",
    };

    public UpdateGovernancePolicyCommandValidator()
    {
        RuleForEach(x => x.AllowedProviders)
            .Must(p => ValidProviders.Contains(p))
            .WithMessage("'{PropertyValue}' is not a valid AI provider.");

        RuleFor(x => x.DefaultProvider)
            .Must(p => string.IsNullOrEmpty(p) || ValidProviders.Contains(p))
            .WithMessage("DefaultProvider must be 'Local', 'Claude', or 'OpenAi'.");

        RuleForEach(x => x.AllowedTaskTypes)
            .Must(t => ValidTaskTypes.Contains(t))
            .WithMessage("'{PropertyValue}' is not a valid AI task type.");

        RuleFor(x => x.MaxDailyRequestsPerUser)
            .GreaterThan(0)
            .When(x => x.MaxDailyRequestsPerUser.HasValue)
            .WithMessage("MaxDailyRequestsPerUser must be greater than 0.");

        RuleFor(x => x.MaxMonthlyRequestsPerTenant)
            .GreaterThan(0)
            .When(x => x.MaxMonthlyRequestsPerTenant.HasValue)
            .WithMessage("MaxMonthlyRequestsPerTenant must be greater than 0.");
    }
}
