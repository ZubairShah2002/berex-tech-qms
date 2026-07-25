using FluentValidation;

namespace BerexQms.Application.Inspection.Queries.ListSamplingPlans;

public sealed class ListSamplingPlansQueryValidator : AbstractValidator<ListSamplingPlansQuery>
{
    public ListSamplingPlansQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}
