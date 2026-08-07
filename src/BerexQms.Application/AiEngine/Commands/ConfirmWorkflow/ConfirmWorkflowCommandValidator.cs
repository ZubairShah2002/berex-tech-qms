using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.ConfirmWorkflow;

public sealed class ConfirmWorkflowCommandValidator
    : AbstractValidator<ConfirmWorkflowCommand>
{
    public ConfirmWorkflowCommandValidator()
    {
        RuleFor(x => x.ExecutionId).NotEmpty();
    }
}
