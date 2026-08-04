using FluentValidation;

namespace BerexQms.Application.SupplierQuality.Commands.RespondToScar;

public sealed class RespondToScarCommandValidator : AbstractValidator<RespondToScarCommand>
{
    public RespondToScarCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier ID is required.");

        RuleFor(x => x.ScarId)
            .NotEmpty().WithMessage("SCAR ID is required.");

        RuleFor(x => x.RootCause)
            .NotEmpty().WithMessage("Root cause analysis is required.")
            .MaximumLength(4000).WithMessage("Root cause must not exceed 4000 characters.");

        RuleFor(x => x.CorrectiveActions)
            .NotEmpty().WithMessage("Corrective actions are required.")
            .MaximumLength(4000).WithMessage("Corrective actions must not exceed 4000 characters.");
    }
}
