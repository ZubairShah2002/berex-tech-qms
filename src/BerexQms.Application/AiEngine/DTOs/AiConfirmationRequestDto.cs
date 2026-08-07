namespace BerexQms.Application.AiEngine.DTOs;

/// <summary>
/// Returned when an AI action requires explicit confirmation before execution.
/// The frontend must display the action summary, affected records, risk level,
/// rollback possibility, and a confirmation prompt.
/// </summary>
public sealed record AiConfirmationRequestDto(
    Guid ActionLogId,
    string ActionType,
    string ActionCategory,
    string RiskLevel,
    string ActionSummary,
    string? AffectedRecords,
    bool IsRollbackPossible,
    string ConfirmationPrompt);
