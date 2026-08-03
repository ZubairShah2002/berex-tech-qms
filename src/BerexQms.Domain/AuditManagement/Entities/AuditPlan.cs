using BerexQms.Domain.AuditManagement.Enums;
using BerexQms.Domain.AuditManagement.Events;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AuditManagement.Entities;

public sealed class AuditPlan : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<AuditRecord> _audits = [];

    public string PlanName { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public string? Description { get; private set; }
    public string? Scope { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<AuditRecord> Audits => _audits.AsReadOnly();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private AuditPlan() { }

    public static AuditPlan Create(
        Guid id,
        TenantId tenantId,
        string planName,
        int year,
        string? description,
        string? scope)
    {
        if (string.IsNullOrWhiteSpace(planName))
            throw new DomainException("Plan name is required.");

        if (year < 2000)
            throw new DomainException("Plan year is invalid.");

        return new AuditPlan
        {
            Id = id,
            TenantId = tenantId,
            PlanName = planName.Trim(),
            Year = year,
            Description = description?.Trim(),
            Scope = scope?.Trim(),
            IsActive = true,
        };
    }

    public void UpdateDetails(string planName, string? description, string? scope)
    {
        if (string.IsNullOrWhiteSpace(planName))
            throw new DomainException("Plan name is required.");

        PlanName = planName.Trim();
        Description = description?.Trim();
        Scope = scope?.Trim();
    }

    public AuditRecord AddAudit(
        string auditNumber,
        AuditType auditType,
        string leadAuditorId,
        string? auditeeArea,
        DateTime scheduledDate)
    {
        if (!IsActive)
            throw new DomainException("Cannot add an audit to an inactive plan.");

        var audit = AuditRecord.Create(
            Guid.NewGuid(), TenantId, Id, auditNumber, auditType, leadAuditorId, auditeeArea, scheduledDate);

        _audits.Add(audit);
        return audit;
    }

    public void StartAudit(Guid auditRecordId)
    {
        var audit = FindAudit(auditRecordId);
        audit.Start();
    }

    public void CompleteAudit(Guid auditRecordId, string summary, string recommendations, string? auditorNotes)
    {
        var audit = FindAudit(auditRecordId);
        audit.Complete(summary, recommendations, auditorNotes);
    }

    public void CancelAudit(Guid auditRecordId)
    {
        var audit = FindAudit(auditRecordId);
        audit.Cancel();
    }

    public AuditFinding RecordFinding(
        Guid auditRecordId,
        FindingClassification findingClassification,
        string clauseReference,
        string description,
        string? evidence,
        string? correctiveAction,
        string? linkedCapaId)
    {
        var audit = FindAudit(auditRecordId);
        var finding = audit.AddFinding(
            findingClassification, clauseReference, description, evidence, correctiveAction, linkedCapaId);

        if (findingClassification is FindingClassification.MajorNonConformance
            or FindingClassification.MinorNonConformance)
        {
            AddDomainEvent(new AuditFindingRecordedEvent(
                finding.Id,
                audit.Id,
                findingClassification.ToString(),
                audit.AuditeeArea ?? string.Empty,
                TenantId.Value));
        }

        return finding;
    }

    public AuditChecklist AddChecklist(
        Guid auditRecordId,
        string standard,
        string clauseReference,
        string requirement,
        bool isCompliant,
        string? evidence,
        string? notes)
    {
        var audit = FindAudit(auditRecordId);
        return audit.AddChecklistItem(standard, clauseReference, requirement, isCompliant, evidence, notes);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private AuditRecord FindAudit(Guid auditRecordId)
    {
        return _audits.FirstOrDefault(a => a.Id == auditRecordId)
            ?? throw new DomainException("Audit record not found.");
    }
}
