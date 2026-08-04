using FluentValidation;

namespace BerexQms.Application.SupplierQuality.Commands.CreateScorecard;

public sealed class CreateScorecardCommandValidator : AbstractValidator<CreateScorecardCommand>
{
    public CreateScorecardCommandValidator()
    {
        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier ID is required.");

        RuleFor(x => x.PeriodStart)
            .NotEmpty().WithMessage("Period start is required.");

        RuleFor(x => x.PeriodEnd)
            .NotEmpty().WithMessage("Period end is required.")
            .GreaterThan(x => x.PeriodStart).WithMessage("Period end must be after period start.");

        RuleFor(x => x.QualityScore)
            .InclusiveBetween(0, 100).WithMessage("Quality score must be between 0 and 100.");

        RuleFor(x => x.DeliveryScore)
            .InclusiveBetween(0, 100).WithMessage("Delivery score must be between 0 and 100.");

        RuleFor(x => x.ResponsivenessScore)
            .InclusiveBetween(0, 100).WithMessage("Responsiveness score must be between 0 and 100.");

        RuleFor(x => x.CostScore)
            .InclusiveBetween(0, 100).WithMessage("Cost score must be between 0 and 100.");
    }
}
