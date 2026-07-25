using FluentValidation;

namespace BerexQms.Application.ProductCatalog.Commands.RemoveBomReference;

public sealed class RemoveBomReferenceCommandValidator : AbstractValidator<RemoveBomReferenceCommand>
{
    public RemoveBomReferenceCommandValidator()
    {
        RuleFor(x => x.PartId)
            .NotEmpty().WithMessage("Part ID is required.");

        RuleFor(x => x.BomReferenceId)
            .NotEmpty().WithMessage("BOM reference ID is required.");
    }
}
