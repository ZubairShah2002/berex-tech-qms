using FluentValidation;

namespace BerexQms.Application.AiEngine.Queries.SearchKnowledgeContext;

public sealed class SearchKnowledgeContextQueryValidator
    : AbstractValidator<SearchKnowledgeContextQuery>
{
    public SearchKnowledgeContextQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty().WithMessage("Search term is required.")
            .MinimumLength(2).WithMessage("Search term must be at least 2 characters.")
            .MaximumLength(500).WithMessage("Search term cannot exceed 500 characters.");

        RuleFor(x => x.MaxResults)
            .InclusiveBetween(1, 100).WithMessage("Max results must be between 1 and 100.");
    }
}
