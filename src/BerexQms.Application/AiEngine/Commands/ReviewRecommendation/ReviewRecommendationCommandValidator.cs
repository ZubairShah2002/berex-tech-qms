using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.ReviewRecommendation;

public sealed class ReviewRecommendationCommandValidator
    : AbstractValidator<ReviewRecommendationCommand>
{
    private static readonly string[] ValidActions = ["accept", "reject", "review"];

    public ReviewRecommendationCommandValidator()
    {
        RuleFor(x => x.RecommendationId)
            .NotEmpty().WithMessage("Recommendation ID is required.");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .Must(a => ValidActions.Contains(a.ToLowerInvariant()))
            .WithMessage("Action must be one of: accept, reject, review.");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2,000 characters.");
    }
}
