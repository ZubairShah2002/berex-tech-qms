using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.ConfirmAiAction;

public sealed class ConfirmAiActionCommandValidator
    : AbstractValidator<ConfirmAiActionCommand>
{
    public ConfirmAiActionCommandValidator()
    {
        RuleFor(x => x.ActionLogId).NotEmpty();
    }
}
