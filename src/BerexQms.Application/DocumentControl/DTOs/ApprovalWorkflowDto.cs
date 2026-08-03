namespace BerexQms.Application.DocumentControl.DTOs;

public sealed record ApprovalWorkflowDto(
    Guid Id,
    Guid DocumentVersionId,
    int CurrentStepOrder,
    bool IsComplete,
    bool IsRejected,
    IReadOnlyList<ApprovalStepDto> Steps,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public sealed record ApprovalStepDto(
    int StepOrder,
    string ApproverId,
    string? Decision,
    string? Comments,
    DateTime? DecidedAt);
