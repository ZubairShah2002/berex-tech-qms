using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.ExecuteAiAction;

public sealed class ExecuteAiActionCommandValidator
    : AbstractValidator<ExecuteAiActionCommand>
{
    public ExecuteAiActionCommandValidator()
    {
        RuleFor(x => x.ActionType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Prompt).MaximumLength(10000);
        RuleFor(x => x.TargetModule).MaximumLength(100);
        RuleFor(x => x.TargetRecordId).MaximumLength(100);
        RuleFor(x => x.TargetRecordType).MaximumLength(100);
        RuleFor(x => x.Parameters).MaximumLength(50000);
    }
}
