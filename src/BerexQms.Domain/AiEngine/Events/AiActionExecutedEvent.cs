using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Events;

public sealed record AiActionExecutedEvent(
    Guid ActionLogId,
    Guid UserId,
    string ActionType,
    string ExecutionResult) : DomainEvent;
