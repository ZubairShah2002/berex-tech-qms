using FluentValidation;

namespace BerexQms.Application.AiEngine.Queries.ListWorkflowExecutions;

public sealed class ListWorkflowExecutionsQueryValidator
    : AbstractValidator<ListWorkflowExecutionsQuery>
{
    public ListWorkflowExecutionsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
