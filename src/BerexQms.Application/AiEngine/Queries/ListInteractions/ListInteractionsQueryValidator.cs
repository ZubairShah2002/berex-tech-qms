using FluentValidation;

namespace BerexQms.Application.AiEngine.Queries.ListInteractions;

public sealed class ListInteractionsQueryValidator : AbstractValidator<ListInteractionsQuery>
{
    public ListInteractionsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}
