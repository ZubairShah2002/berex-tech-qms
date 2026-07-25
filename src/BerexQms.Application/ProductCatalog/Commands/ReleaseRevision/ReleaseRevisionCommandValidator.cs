using FluentValidation;

namespace BerexQms.Application.ProductCatalog.Commands.ReleaseRevision;

public sealed class ReleaseRevisionCommandValidator : AbstractValidator<ReleaseRevisionCommand>
{
    public ReleaseRevisionCommandValidator()
    {
        RuleFor(x => x.PartId)
            .NotEmpty().WithMessage("Part ID is required.");

        RuleFor(x => x.RevisionId)
            .NotEmpty().WithMessage("Revision ID is required.");
    }
}
