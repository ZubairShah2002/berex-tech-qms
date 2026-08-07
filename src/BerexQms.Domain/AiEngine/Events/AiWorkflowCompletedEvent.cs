using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Events;

public sealed record AiWorkflowCompletedEvent(
    Guid WorkflowExecutionId,
    string WorkflowName,
    Guid UserId,
    string Status,
    int CompletedSteps,
    int FailedSteps) : DomainEvent;
