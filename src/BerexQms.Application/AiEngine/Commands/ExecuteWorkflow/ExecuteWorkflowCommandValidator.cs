using FluentValidation;

namespace BerexQms.Application.AiEngine.Commands.ExecuteWorkflow;

public sealed class ExecuteWorkflowCommandValidator
    : AbstractValidator<ExecuteWorkflowCommand>
{
    public ExecuteWorkflowCommandValidator()
    {
        RuleFor(x => x.WorkflowDefinitionId).NotEmpty();
    }
}
