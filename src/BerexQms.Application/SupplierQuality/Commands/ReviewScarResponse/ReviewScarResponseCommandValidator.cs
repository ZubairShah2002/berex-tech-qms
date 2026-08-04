using FluentValidation;

namespace BerexQms.Application.SupplierQuality.Commands.ReviewScarResponse;

public sealed class ReviewScarResponseCommandValidator : AbstractValidator<ReviewScarResponseCommand>
{
    public ReviewScarResponseCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier ID is required.");

        RuleFor(x => x.ScarId)
            .NotEmpty().WithMessage("SCAR ID is required.");

        RuleFor(x => x.Decision)
            .NotEmpty().WithMessage("Decision is required.")
            .Must(d => d.Equals("Accept", StringComparison.OrdinalIgnoreCase)
                     || d.Equals("Reject", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Decision must be 'Accept' or 'Reject'.");
    }
}
