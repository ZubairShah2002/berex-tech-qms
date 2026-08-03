using BerexQms.Domain.Capa.Enums;
using BerexQms.Domain.Capa.Events;
using BerexQms.Domain.Capa.ValueObjects;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Capa.Entities;

public sealed class CAPARecord : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<CapaAction> _actions = [];
    private readonly List<EffectivenessVerification> _verifications = [];

    public string CapaNumber { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public CAPAStatus Status { get; private set; }
    public CAPAPriority Priority { get; private set; }
    public CAPASource Source { get; private set; } = null!;

    public string OwnerId { get; private set; } = string.Empty;
    public string? AssignedTo { get; private set; }
    public Guid? SourceNonConformanceId { get; private set; }

    public Guid? RootCauseAnalysisId { get; private set; }
    public RootCauseAnalysis? RootCauseAnalysis { get; private set; }

    public DateTime? TargetClosureDate { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public string? ClosedBy { get; private set; }
    public string? ClosureNotes { get; private set; }

    public IReadOnlyCollection<CapaAction> Actions => _actions.AsReadOnly();
    public IReadOnlyCollection<EffectivenessVerification> Verifications => _verifications.AsReadOnly();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private CAPARecord() { }

    public static CAPARecord Initiate(
        Guid id,
        TenantId tenantId,
        string capaNumber,
        string title,
        string description,
        CAPAPriority priority,
        CAPASource source,
        string ownerId,
        Guid? sourceNonConformanceId,
        DateTime? targetClosureDate)
    {
        if (string.IsNullOrWhiteSpace(capaNumber))
            throw new DomainException("CAPA number is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("CAPA title is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("CAPA description is required.");

        if (string.IsNullOrWhiteSpace(ownerId))
            throw new DomainException("CAPA owner is required.");

        var record = new CAPARecord
        {
            Id = id,
            TenantId = tenantId,
            CapaNumber = capaNumber.Trim().ToUpperInvariant(),
            Title = title.Trim(),
            Description = description.Trim(),
            Status = CAPAStatus.Initiated,
            Priority = priority,
            Source = source,
            OwnerId = ownerId,
            SourceNonConformanceId = sourceNonConformanceId,
            TargetClosureDate = targetClosureDate,
        };

        record.AddDomainEvent(new CAPAInitiatedEvent(
            id, record.CapaNumber, source.SourceType.ToString(),
            sourceNonConformanceId, ownerId, tenantId.Value));

        return record;
    }

    public void StartRCA(RCAMethodology methodology, string analystId)
    {
        if (Status != CAPAStatus.Initiated && Status != CAPAStatus.RCAInProgress)
            throw new DomainException($"Cannot start RCA in status: {Status}.");

        var rca = RootCauseAnalysis.Create(
            Guid.NewGuid(), TenantId, Id, methodology, analystId);

        RootCauseAnalysis = rca;
        RootCauseAnalysisId = rca.Id;
        Status = CAPAStatus.RCAInProgress;
    }

    public void SubmitRCA(string rootCause, string? analysisDetails, string? contributingFactors)
    {
        if (Status != CAPAStatus.RCAInProgress)
            throw new DomainException($"Cannot submit RCA in status: {Status}.");

        if (RootCauseAnalysis is null)
            throw new DomainException("No active RCA found. Start RCA first.");

        RootCauseAnalysis.SubmitFindings(rootCause, analysisDetails, contributingFactors);
        Status = CAPAStatus.ActionPlanning;
    }

    public CapaAction AddAction(
        ActionType actionType, string description, string ownerId,
        DateTime dueDate, string? evidenceRequirement)
    {
        if (Status != CAPAStatus.ActionPlanning && Status != CAPAStatus.Implementation)
            throw new DomainException($"Cannot add actions in status: {Status}.");

        var action = CapaAction.Create(
            Guid.NewGuid(), TenantId, Id,
            actionType, description, ownerId, dueDate, evidenceRequirement);

        _actions.Add(action);

        if (Status == CAPAStatus.ActionPlanning)
            Status = CAPAStatus.Implementation;

        return action;
    }

    public void CompleteAction(Guid actionId, string completedBy, string? completionNotes, string? evidenceProvided)
    {
        if (Status != CAPAStatus.Implementation)
            throw new DomainException($"Cannot complete actions in status: {Status}.");

        var action = _actions.FirstOrDefault(a => a.Id == actionId)
            ?? throw new DomainException("Action not found.");

        action.Complete(completedBy, completionNotes, evidenceProvided);

        if (_actions.All(a => a.CompletedAt is not null))
            Status = CAPAStatus.PendingVerification;
    }

    public EffectivenessVerification ScheduleVerification(
        DateTime scheduledDate, string verificationCriteria)
    {
        if (Status != CAPAStatus.Implementation && Status != CAPAStatus.PendingVerification)
            throw new DomainException($"Cannot schedule verification in status: {Status}.");

        var verification = EffectivenessVerification.Schedule(
            Guid.NewGuid(), TenantId, Id, scheduledDate, verificationCriteria);

        _verifications.Add(verification);
        return verification;
    }

    public void RecordVerification(Guid verificationId, string verifierId, bool isEffective, string result, string? evidence)
    {
        if (Status != CAPAStatus.PendingVerification)
            throw new DomainException($"Cannot record verification in status: {Status}.");

        var verification = _verifications.FirstOrDefault(v => v.Id == verificationId)
            ?? throw new DomainException("Verification not found.");

        verification.RecordResult(verifierId, isEffective, result, evidence);

        if (isEffective)
        {
            Status = CAPAStatus.ClosedEffective;
            ClosedAt = DateTime.UtcNow;
            ClosedBy = verifierId;

            AddDomainEvent(new CAPAClosedEvent(
                Id, CapaNumber, "ClosedEffective", TenantId.Value));
        }
        else
        {
            Status = CAPAStatus.RCAInProgress;
            RootCauseAnalysis = null;
            RootCauseAnalysisId = null;
        }
    }

    public void AssignTo(string assigneeId)
    {
        if (string.IsNullOrWhiteSpace(assigneeId))
            throw new DomainException("Assignee ID is required.");

        AssignedTo = assigneeId;
    }

    public bool HasOverdueActions => _actions.Any(a => a.IsOverdue);
}
