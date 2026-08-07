using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Commands.ConfirmWorkflow;

public sealed record ConfirmWorkflowCommand(
    Guid ExecutionId,
    bool Confirm) : ICommand<AiWorkflowExecutionDto>;
