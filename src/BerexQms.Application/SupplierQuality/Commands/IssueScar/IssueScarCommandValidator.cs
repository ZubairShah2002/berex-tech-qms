using FluentValidation;

namespace BerexQms.Application.SupplierQuality.Commands.IssueScar;

public sealed class IssueScarCommandValidator : AbstractValidator<IssueScarCommand>
{
    public IssueScarCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier ID is required.");

        RuleFor(x => x.ScarNumber)
            .NotEmpty().WithMessage("SCAR number is required.")
            .MaximumLength(50).WithMessage("SCAR number must not exceed 50 characters.");

        RuleFor(x => x.DefectDescription)
            .NotEmpty().WithMessage("Defect description is required.")
            .MaximumLength(4000).WithMessage("Defect description must not exceed 4000 characters.");

        RuleFor(x => x.Severity)
            .NotEmpty().WithMessage("Severity is required.")
            .MaximumLength(50).WithMessage("Severity must not exceed 50 characters.");

        RuleFor(x => x.ResponseDays)
            .GreaterThan(0).WithMessage("Response days must be greater than 0.");
    }
}
