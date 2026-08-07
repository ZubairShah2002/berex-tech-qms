namespace BerexQms.Application.AiEngine.DTOs;

public sealed record AiWorkflowExecutionDto(
    Guid Id,
    Guid WorkflowDefinitionId,
    string WorkflowName,
    Guid UserId,
    string Status,
    int TotalSteps,
    int CompletedSteps,
    int FailedSteps,
    string? Output,
    DateTime StartedAt,
    DateTime? CompletedAt,
    int? TotalDurationMs,
    string? ErrorSummary);
