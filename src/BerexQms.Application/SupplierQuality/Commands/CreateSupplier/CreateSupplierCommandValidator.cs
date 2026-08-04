using FluentValidation;

namespace BerexQms.Application.SupplierQuality.Commands.CreateSupplier;

public sealed class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Supplier code is required.")
            .MaximumLength(50).WithMessage("Supplier code must not exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Supplier name is required.")
            .MaximumLength(200).WithMessage("Supplier name must not exceed 200 characters.");

        RuleFor(x => x.Tier)
            .MaximumLength(50).WithMessage("Tier must not exceed 50 characters.")
            .When(x => x.Tier is not null);

        RuleFor(x => x.ContactEmail)
            .EmailAddress().WithMessage("Contact email must be a valid email address.")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

        RuleFor(x => x.ContactName)
            .MaximumLength(200).WithMessage("Contact name must not exceed 200 characters.")
            .When(x => x.ContactName is not null);
    }
}
