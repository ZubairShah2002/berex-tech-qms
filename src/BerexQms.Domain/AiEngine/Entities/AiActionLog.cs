using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Events;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AiEngine.Entities;

/// <summary>
/// Immutable audit record capturing every AI-initiated action. This is the
/// enhanced AI audit trail required by the v2.0 specification — separate from
/// the standard audit_log and the existing AiInteraction audit. Captures user,
/// role, permission level, prompt, reasoning, affected records, confirmation
/// status, and execution outcome.
/// </summary>
public sealed class AiActionLog : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid UserId { get; private set; }
    public string UserRole { get; private set; } = string.Empty;
    public string PermissionLevel { get; private set; } = string.Empty;
    public string ActionType { get; private set; } = string.Empty;
    public string ActionCategory { get; private set; } = string.Empty;
    public string? Prompt { get; private set; }
    public string? ReasoningSummary { get; private set; }
    public string? AffectedModules { get; private set; }
    public string? AffectedRecords { get; private set; }
    public string RiskLevel { get; private set; } = string.Empty;
    public string ConfirmationStatus { get; private set; } = string.Empty;
    public bool RequiresConfirmation { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public string? ConfirmedBy { get; private set; }
    public string ExecutionResult { get; private set; } = string.Empty;
    public string? ErrorDetail { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int? DurationMs { get; private set; }
    public string? ModelVersion { get; private set; }
    public decimal? ConfidenceScore { get; private set; }
    public bool IsRollbackPossible { get; private set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private AiActionLog() { }

    public static AiActionLog Create(
        Guid id,
        TenantId tenantId,
        Guid userId,
        string userRole,
        AiPermissionLevel permissionLevel,
        AiActionType actionType,
        AiActionCategory actionCategory,
        string? prompt,
        Enums.RiskLevel riskLevel,
        bool requiresConfirmation)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User ID is required for an AI action log entry.");

        return new AiActionLog
        {
            Id = id,
            TenantId = tenantId,
            UserId = userId,
            UserRole = userRole,
            PermissionLevel = permissionLevel.ToString(),
            ActionType = actionType.ToString(),
            ActionCategory = actionCategory.ToString(),
            Prompt = prompt,
            RiskLevel = riskLevel.ToString(),
            RequiresConfirmation = requiresConfirmation,
            ConfirmationStatus = requiresConfirmation
                ? Enums.ConfirmationStatus.Pending.ToString()
                : Enums.ConfirmationStatus.Confirmed.ToString(),
            ExecutionResult = requiresConfirmation ? "AwaitingConfirmation" : "Pending",
            RequestedAt = DateTime.UtcNow,
            IsRollbackPossible = false,
        };
    }

    public void RecordConfirmation(string confirmedByUserId)
    {
        if (ConfirmationStatus != Enums.ConfirmationStatus.Pending.ToString())
            throw new DomainException("Only a pending action can be confirmed.");

        ConfirmationStatus = Enums.ConfirmationStatus.Confirmed.ToString();
        ConfirmedAt = DateTime.UtcNow;
        ConfirmedBy = confirmedByUserId;
        ExecutionResult = "Pending";
    }

    public void RejectConfirmation(string rejectedByUserId)
    {
        if (ConfirmationStatus != Enums.ConfirmationStatus.Pending.ToString())
            throw new DomainException("Only a pending action can be rejected.");

        ConfirmationStatus = Enums.ConfirmationStatus.Rejected.ToString();
        ConfirmedAt = DateTime.UtcNow;
        ConfirmedBy = rejectedByUserId;
        ExecutionResult = "Rejected";
        CompletedAt = DateTime.UtcNow;
    }

    public void RecordSuccess(
        string? reasoningSummary,
        string? affectedModules,
        string? affectedRecords,
        int durationMs,
        string? modelVersion,
        decimal? confidenceScore,
        bool isRollbackPossible)
    {
        if (ConfirmationStatus != Enums.ConfirmationStatus.Confirmed.ToString())
            throw new DomainException("Action must be confirmed before recording success.");

        ReasoningSummary = reasoningSummary;
        AffectedModules = affectedModules;
        AffectedRecords = affectedRecords;
        ExecutionResult = "Success";
        CompletedAt = DateTime.UtcNow;
        DurationMs = durationMs;
        ModelVersion = modelVersion;
        ConfidenceScore = confidenceScore;
        IsRollbackPossible = isRollbackPossible;

        AddDomainEvent(new AiActionExecutedEvent(Id, UserId, ActionType, "Success"));
    }

    public void RecordFailure(string errorDetail, int durationMs)
    {
        ExecutionResult = "Failed";
        ErrorDetail = errorDetail;
        CompletedAt = DateTime.UtcNow;
        DurationMs = durationMs;

        AddDomainEvent(new AiActionExecutedEvent(Id, UserId, ActionType, "Failed"));
    }

    public void ExpireConfirmation()
    {
        if (ConfirmationStatus != Enums.ConfirmationStatus.Pending.ToString())
            throw new DomainException("Only a pending action can expire.");

        ConfirmationStatus = Enums.ConfirmationStatus.Expired.ToString();
        ExecutionResult = "Expired";
        CompletedAt = DateTime.UtcNow;
    }
}
