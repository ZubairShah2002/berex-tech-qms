using FluentValidation;

namespace BerexQms.Application.ProductCatalog.Commands.AddBomReference;

public sealed class AddBomReferenceCommandValidator : AbstractValidator<AddBomReferenceCommand>
{
    public AddBomReferenceCommandValidator()
    {
        RuleFor(x => x.PartId)
            .NotEmpty().WithMessage("Part ID is required.");

        RuleFor(x => x.ChildPartId)
            .NotEmpty().WithMessage("Child part ID is required.")
            .NotEqual(x => x.PartId).WithMessage("A part cannot reference itself in a BOM.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.ReferenceDesignator)
            .MaximumLength(100).WithMessage("Reference designator cannot exceed 100 characters.")
            .When(x => x.ReferenceDesignator is not null);
    }
}
