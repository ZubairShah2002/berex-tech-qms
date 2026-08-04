using FluentValidation;

namespace BerexQms.Application.Calibration.Commands.ReviewImpactAssessment;

public sealed class ReviewImpactAssessmentCommandValidator : AbstractValidator<ReviewImpactAssessmentCommand>
{
    private static readonly string[] ValidActions = ["REVIEW", "CLOSE"];

    public ReviewImpactAssessmentCommandValidator()
    {
        RuleFor(x => x.AssessmentId).NotEmpty();
        RuleFor(x => x.Action).NotEmpty()
            .Must(a => ValidActions.Contains(a.ToUpperInvariant()))
            .WithMessage("Action must be REVIEW or CLOSE.");
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
