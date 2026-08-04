using FluentValidation;

namespace BerexQms.Application.SupplierQuality.Commands.AddApproval;

public sealed class AddApprovalCommandValidator : AbstractValidator<AddApprovalCommand>
{
    public AddApprovalCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier ID is required.");

        RuleFor(x => x.ScopeDescription)
            .NotEmpty().WithMessage("Scope description is required.")
            .MaximumLength(2000).WithMessage("Scope description must not exceed 2000 characters.");

        RuleFor(x => x.ApprovedDate)
            .NotEmpty().WithMessage("Approved date is required.");

        RuleFor(x => x.Conditions)
            .MaximumLength(2000).WithMessage("Conditions must not exceed 2000 characters.")
            .When(x => x.Conditions is not null);
    }
}
