using FluentValidation;

namespace BerexQms.Application.NonConformance.Queries.FindSimilarNonConformances;

public sealed class FindSimilarNonConformancesQueryValidator
    : AbstractValidator<FindSimilarNonConformancesQuery>
{
    public FindSimilarNonConformancesQueryValidator()
    {
        RuleFor(x => x.NonConformanceId)
            .NotEmpty().WithMessage("Non-conformance ID is required.");

        RuleFor(x => x.LookbackDays)
            .InclusiveBetween(1, 365).WithMessage("Lookback days must be between 1 and 365.");
    }
}
