using FluentValidation;

namespace BerexQms.Application.AiEngine.Queries.ListAiActionLogs;

public sealed class ListAiActionLogsQueryValidator
    : AbstractValidator<ListAiActionLogsQuery>
{
    public ListAiActionLogsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
