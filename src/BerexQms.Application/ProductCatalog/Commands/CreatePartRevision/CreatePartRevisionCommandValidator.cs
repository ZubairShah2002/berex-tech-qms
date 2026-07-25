using FluentValidation;

namespace BerexQms.Application.ProductCatalog.Commands.CreatePartRevision;

public sealed class CreatePartRevisionCommandValidator : AbstractValidator<CreatePartRevisionCommand>
{
    public CreatePartRevisionCommandValidator()
    {
        RuleFor(x => x.PartId)
            .NotEmpty().WithMessage("Part ID is required.");

        RuleFor(x => x.RevisionCode)
            .NotEmpty().WithMessage("Revision code is required.")
            .MaximumLength(20).WithMessage("Revision code cannot exceed 20 characters.")
            .Matches("^[A-Za-z0-9._-]+$").WithMessage("Revision code must contain only alphanumeric characters, dots, hyphens, or underscores.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.ChangeReason)
            .MaximumLength(1000).WithMessage("Change reason cannot exceed 1000 characters.")
            .When(x => x.ChangeReason is not null);
    }
}
