using FluentValidation;

namespace BerexQms.Application.SupplierQuality.Commands.UpdateSupplier;

public sealed class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier name is required.")
            .MaximumLength(200).WithMessage("Supplier name must not exceed 200 characters.");

        RuleFor(x => x.Tier)
            .MaximumLength(50).WithMessage("Tier must not exceed 50 characters.")
            .When(x => x.Tier is not null);

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("Contact email must be a valid email address.")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
    }
}
