using BerexQms.Domain.ProductCatalog.Enums;
using FluentValidation;

namespace BerexQms.Application.ProductCatalog.Commands.UpdatePart;

public sealed class UpdatePartCommandValidator : AbstractValidator<UpdatePartCommand>
{
    public UpdatePartCommandValidator()
    {
        RuleFor(x => x.PartId)
            .NotEmpty().WithMessage("Part ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Part name is required.")
            .MaximumLength(200).WithMessage("Part name cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.")
            .When(x => x.Description is not null);

        RuleFor(x => x.ProductFamily)
            .MaximumLength(100).WithMessage("Product family cannot exceed 100 characters.")
            .When(x => x.ProductFamily is not null);

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category cannot exceed 100 characters.")
            .When(x => x.Category is not null);

        RuleFor(x => x.SerializationMode)
            .Must(v => string.IsNullOrEmpty(v) || Enum.TryParse<SerializationMode>(v, true, out _))
            .WithMessage("Invalid serialization mode. Valid values: None, Lot, Serial, LotAndSerial.");

        RuleFor(x => x.UnitOfMeasure)
            .MaximumLength(20).WithMessage("Unit of measure cannot exceed 20 characters.")
            .When(x => x.UnitOfMeasure is not null);
    }
}
