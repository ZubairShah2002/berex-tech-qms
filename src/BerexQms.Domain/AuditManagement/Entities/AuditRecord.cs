using BerexQms.Domain.AuditManagement.Enums;
using BerexQms.Domain.AuditManagement.ValueObjects;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AuditManagement.Entities;

public sealed class AuditRecord : Entity<Guid>
{
    private readonly List<AuditFinding> _findings = [];
    private readonly List<AuditChecklist> _checklists = [];

    public Guid AuditPlanId { get; private set; }
    public string AuditNumber { get; private set; } = string.Empty;
    public AuditType AuditType { get; private set; }
    public AuditStatus Status { get; private set; }
    public string LeadAuditorId { get; private set; } = string.Empty;
    public string? AuditeeArea { get; private set; }
    public DateTime ScheduledDate { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public AuditReport? Report { get; private set; }

    public IReadOnlyCollection<AuditFinding> Findings => _findings.AsReadOnly();
    public IReadOnlyCollection<AuditChecklist> Checklists => _checklists.AsReadOnly();

    private AuditRecord() { }

    internal static AuditRecord Create(
        Guid id,
        TenantId tenantId,
        Guid auditPlanId,
        string auditNumber,
        AuditType auditType,
        string leadAuditorId,
        string? auditeeArea,
        DateTime scheduledDate)
    {
        if (string.IsNullOrWhiteSpace(auditNumber))
            throw new DomainException("Audit number is required.");

        if (string.IsNullOrWhiteSpace(leadAuditorId))
            throw new DomainException("Lead auditor is required.");

        return new AuditRecord
        {
            Id = id,
            TenantId = tenantId,
            AuditPlanId = auditPlanId,
            AuditNumber = auditNumber.Trim().ToUpperInvariant(),
            AuditType = auditType,
            Status = AuditStatus.Planned,
            LeadAuditorId = leadAuditorId,
            AuditeeArea = auditeeArea?.Trim(),
            ScheduledDate = scheduledDate,
        };
    }

    internal void Start()
    {
        if (Status != AuditStatus.Planned)
            throw new DomainException($"Cannot start audit in status: {Status}.");

        Status = AuditStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    internal void Complete(string summary, string recommendations, string? auditorNotes)
    {
        if (Status != AuditStatus.InProgress)
            throw new DomainException($"Cannot complete audit in status: {Status}.");

        if (string.IsNullOrWhiteSpace(summary))
            throw new DomainException("Audit report summary is required.");

        if (string.IsNullOrWhiteSpace(recommendations))
            throw new DomainException("Audit report recommendations are required.");

        Status = AuditStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Report = new AuditReport(summary.Trim(), recommendations.Trim(), auditorNotes?.Trim(), DateTime.UtcNow);
    }

    internal void Cancel()
    {
        if (Status is AuditStatus.Completed or AuditStatus.Cancelled)
            throw new DomainException($"Cannot cancel audit in status: {Status}.");

        Status = AuditStatus.Cancelled;
    }

    internal AuditFinding AddFinding(
        FindingClassification findingClassification,
        string clauseReference,
        string description,
        string? evidence,
        string? correctiveAction,
        string? linkedCapaId)
    {
        if (Status is AuditStatus.Cancelled)
            throw new DomainException("Cannot record findings on a cancelled audit.");

        var finding = AuditFinding.Create(
            Guid.NewGuid(), TenantId, Id, findingClassification,
            clauseReference, description, evidence, correctiveAction, linkedCapaId);

        _findings.Add(finding);
        return finding;
    }

    internal AuditChecklist AddChecklistItem(
        string standard,
        string clauseReference,
        string requirement,
        bool isCompliant,
        string? evidence,
        string? notes)
    {
        var checklist = AuditChecklist.Create(
            Guid.NewGuid(), TenantId, Id, standard, clauseReference, requirement, isCompliant, evidence, notes);

        _checklists.Add(checklist);
        return checklist;
    }
}
