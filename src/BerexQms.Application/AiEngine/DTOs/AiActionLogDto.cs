namespace BerexQms.Application.AiEngine.DTOs;

public sealed record AiActionLogDto(
    Guid Id,
    Guid UserId,
    string UserRole,
    string PermissionLevel,
    string ActionType,
    string ActionCategory,
    string? Prompt,
    string? ReasoningSummary,
    string? AffectedModules,
    string? AffectedRecords,
    string RiskLevel,
    string ConfirmationStatus,
    bool RequiresConfirmation,
    string ExecutionResult,
    string? ErrorDetail,
    DateTime RequestedAt,
    DateTime? CompletedAt,
    int? DurationMs,
    string? ModelVersion,
    decimal? ConfidenceScore,
    bool IsRollbackPossible);
