using FluentValidation;

namespace BerexQms.Application.SupplierQuality.Commands.AddApprovedPart;

public sealed class AddApprovedPartCommandValidator : AbstractValidator<AddApprovedPartCommand>
{
    public AddApprovedPartCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier ID is required.");

        RuleFor(x => x.PartId)
            .NotEmpty().WithMessage("Part ID is required.");

        RuleFor(x => x.ApprovalDate)
            .NotEmpty().WithMessage("Approval date is required.");

        RuleFor(x => x.RevisionScope)
            .MaximumLength(200).WithMessage("Revision scope must not exceed 200 characters.")
            .When(x => x.RevisionScope is not null);
    }
}
