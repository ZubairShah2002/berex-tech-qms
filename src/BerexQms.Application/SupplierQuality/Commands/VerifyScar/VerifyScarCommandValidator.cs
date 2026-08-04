using FluentValidation;

namespace BerexQms.Application.SupplierQuality.Commands.VerifyScar;

public sealed class VerifyScarCommandValidator : AbstractValidator<VerifyScarCommand>
{
    public VerifyScarCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier ID is required.");

        RuleFor(x => x.ScarId)
            .NotEmpty().WithMessage("SCAR ID is required.");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .Must(a => a.Equals("Close", StringComparison.OrdinalIgnoreCase)
                     || a.Equals("FollowUp", StringComparison.OrdinalIgnoreCase)
                     || a.Equals("Reissue", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Action must be 'Close', 'FollowUp', or 'Reissue'.");
    }
}
