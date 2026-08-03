using FluentValidation;

namespace BerexQms.Application.DocumentControl.Commands.SubmitForReview;

public sealed class SubmitForReviewCommandValidator : AbstractValidator<SubmitForReviewCommand>
{
    public SubmitForReviewCommandValidator()
    {
        RuleFor(x => x.DocumentId).NotEmpty();
        RuleFor(x => x.VersionId).NotEmpty();
    }
}
