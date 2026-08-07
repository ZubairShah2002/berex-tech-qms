using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Events;
using BerexQms.Domain.AiEngine.ValueObjects;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AiEngine.Entities;

/// <summary>
/// Records a single interaction with an AI capability, forming the dedicated
/// AI audit trail required by the blueprint (Chapter 19.4). AI assists, never
/// decides — every suggestion is recorded here with its confidence score and
/// the human's eventual action (accepted/rejected/modified/ignored/suppressed).
/// </summary>
public sealed class AiInteraction : AggregateRoot<Guid>, IAuditableEntity
{
    public string Capability { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public string ModelId { get; private set; } = string.Empty;
    public string? InputSummary { get; private set; }
    public string? OutputSummary { get; private set; }
    public ConfidenceScore? Confidence { get; private set; }
    public string? SourceReferences { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string? UserAction { get; private set; }
    public string? UserJustification { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int? ResponseTimeMs { get; private set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private AiInteraction() { }

    public static AiInteraction Create(
        Guid id,
        TenantId tenantId,
        AiCapabilityType capability,
        Guid userId,
        string modelId,
        string? inputSummary)
    {
        if (userId == Guid.Empty)
            throw new DomainException("A user is required to record an AI interaction.");
        if (string.IsNullOrWhiteSpace(modelId))
            throw new DomainException("Model id is required to record an AI interaction.");

        return new AiInteraction
        {
            Id = id,
            TenantId = tenantId,
            Capability = capability.ToString(),
            UserId = userId,
            ModelId = modelId.Trim(),
            InputSummary = inputSummary,
            Status = AiInteractionStatus.Pending.ToString(),
            RequestedAt = DateTime.UtcNow,
        };
    }

    public void Complete(string outputSummary, decimal confidenceScore, string? sourceReferences, int responseTimeMs)
    {
        if (Status != AiInteractionStatus.Pending.ToString())
            throw new DomainException("Only a pending AI interaction can be completed.");
        if (string.IsNullOrWhiteSpace(outputSummary))
            throw new DomainException("Output summary is required to complete an AI interaction.");
        if (responseTimeMs < 0)
            throw new DomainException("Response time cannot be negative.");

        OutputSummary = outputSummary;
        Confidence = ConfidenceScore.Create(confidenceScore);
        SourceReferences = sourceReferences;
        Status = AiInteractionStatus.Completed.ToString();
        CompletedAt = DateTime.UtcNow;
        ResponseTimeMs = responseTimeMs;

        AddDomainEvent(new AiSuggestionCompletedEvent(Id, Capability, Confidence.Score, UserId));
    }

    public void MarkFailed(string? errorSummary)
    {
        if (Status != AiInteractionStatus.Pending.ToString())
            throw new DomainException("Only a pending AI interaction can be marked as failed.");

        OutputSummary = errorSummary;
        Status = AiInteractionStatus.Failed.ToString();
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkTimedOut()
    {
        if (Status != AiInteractionStatus.Pending.ToString())
            throw new DomainException("Only a pending AI interaction can be marked as timed out.");

        Status = AiInteractionStatus.TimedOut.ToString();
        CompletedAt = DateTime.UtcNow;
    }

    public void RecordUserAction(AiUserAction action, string? justification)
    {
        if (Status != AiInteractionStatus.Completed.ToString())
            throw new DomainException("A user action can only be recorded on a completed AI interaction.");

        if (action == AiUserAction.Accepted
            && Confidence is not null
            && Confidence.Level == ConfidenceLevel.Moderate
            && string.IsNullOrWhiteSpace(justification))
        {
            throw new DomainException("A justification is required to accept a moderate-confidence AI suggestion.");
        }

        UserAction = action.ToString();
        UserJustification = justification;
    }

    public void Suppress()
    {
        UserAction = AiUserAction.Suppressed.ToString();
    }
}
