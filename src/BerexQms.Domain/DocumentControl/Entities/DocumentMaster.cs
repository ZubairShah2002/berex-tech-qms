using BerexQms.Domain.DocumentControl.Enums;
using BerexQms.Domain.DocumentControl.Events;
using BerexQms.Domain.DocumentControl.ValueObjects;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.DocumentControl.Entities;

public sealed class DocumentMaster : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<DocumentVersion> _versions = [];

    public string DocumentNumber { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public string OwnerId { get; private set; } = string.Empty;
    public string? Department { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<DocumentVersion> Versions => _versions.AsReadOnly();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private DocumentMaster() { }

    public static DocumentMaster Create(
        Guid id,
        TenantId tenantId,
        string documentNumber,
        string title,
        DocumentType documentType,
        string ownerId,
        string? description,
        string? department)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
            throw new DomainException("Document number is required.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Document title is required.");

        if (string.IsNullOrWhiteSpace(ownerId))
            throw new DomainException("Document owner is required.");

        var doc = new DocumentMaster
        {
            Id = id,
            TenantId = tenantId,
            DocumentNumber = documentNumber.Trim().ToUpperInvariant(),
            Title = title.Trim(),
            Description = description?.Trim(),
            DocumentType = documentType,
            OwnerId = ownerId,
            Department = department?.Trim(),
            IsActive = true,
        };

        doc.AddDomainEvent(new DocumentCreatedEvent(
            id, doc.DocumentNumber, doc.Title,
            documentType.ToString(), ownerId, tenantId.Value));

        return doc;
    }

    public void UpdateDetails(string title, string? description, string? department)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Document title is required.");

        Title = title.Trim();
        Description = description?.Trim();
        Department = department?.Trim();
    }

    public DocumentVersion CreateVersion(
        string versionNumber,
        string content,
        string authorId,
        string? changeDescription)
    {
        var hasReleasedVersion = _versions.Any(v => v.Status == DocumentStatus.Released);
        if (hasReleasedVersion)
        {
            var hasUnreleasedVersion = _versions.Any(v =>
                v.Status != DocumentStatus.Released &&
                v.Status != DocumentStatus.Superseded &&
                v.Status != DocumentStatus.Obsolete);
            if (hasUnreleasedVersion)
                throw new DomainException("An unreleased version already exists. Complete or discard it before creating a new version.");
        }

        var version = DocumentVersion.Create(
            Guid.NewGuid(), TenantId, Id, versionNumber, content, authorId, changeDescription);

        _versions.Add(version);
        return version;
    }

    public void SubmitVersionForReview(Guid versionId)
    {
        var version = FindVersion(versionId);
        version.SubmitForReview();
    }

    public ApprovalWorkflow StartApproval(Guid versionId, IReadOnlyList<string> approverIds)
    {
        var version = FindVersion(versionId);
        version.MoveToApproval();

        var workflow = ApprovalWorkflow.Create(
            Guid.NewGuid(), TenantId, versionId, approverIds);

        return workflow;
    }

    public void RecordApprovalDecision(
        Guid versionId,
        ApprovalWorkflow workflow,
        string approverId,
        ApprovalDecision decision,
        string? comments,
        string? signature)
    {
        var version = FindVersion(versionId);
        if (version.Status != DocumentStatus.PendingApproval)
            throw new DomainException($"Version is not pending approval: {version.Status}.");

        workflow.RecordDecision(approverId, decision, comments, signature);

        if (workflow.IsRejected)
            version.RevertToDraft();

        if (workflow.IsComplete && !workflow.IsRejected)
        {
            AddDomainEvent(new DocumentApprovedEvent(
                Id, DocumentNumber, version.VersionNumber,
                approverId, DateTime.UtcNow, TenantId.Value));
        }
    }

    public void ReleaseVersion(Guid versionId, string releasedBy, DateTime effectiveDate)
    {
        var version = FindVersion(versionId);

        foreach (var existing in _versions.Where(v => v.Status == DocumentStatus.Released))
        {
            existing.Supersede();
        }

        version.Release(releasedBy, effectiveDate);

        AddDomainEvent(new VersionReleasedEvent(
            Id, DocumentNumber, version.VersionNumber,
            releasedBy, effectiveDate, TenantId.Value));
    }

    public Distribution AddDistribution(
        Guid versionId,
        string recipientId,
        DateTime complianceDeadline)
    {
        var version = FindVersion(versionId);
        if (version.Status != DocumentStatus.Released)
            throw new DomainException("Can only distribute released versions.");

        var distribution = Distribution.Create(
            Guid.NewGuid(), TenantId, versionId, recipientId, complianceDeadline);

        return distribution;
    }

    public void MakeObsolete()
    {
        foreach (var version in _versions.Where(v =>
            v.Status != DocumentStatus.Obsolete && v.Status != DocumentStatus.Superseded))
        {
            version.MakeObsolete();
        }

        IsActive = false;
    }

    private DocumentVersion FindVersion(Guid versionId)
    {
        return _versions.FirstOrDefault(v => v.Id == versionId)
            ?? throw new DomainException("Document version not found.");
    }
}
