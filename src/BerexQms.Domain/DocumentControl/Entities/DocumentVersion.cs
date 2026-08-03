using BerexQms.Domain.DocumentControl.Enums;
using BerexQms.Domain.DocumentControl.ValueObjects;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.DocumentControl.Entities;

public sealed class DocumentVersion : Entity<Guid>
{
    public Guid DocumentMasterId { get; private set; }
    public string VersionNumber { get; private set; } = string.Empty;
    public DocumentStatus Status { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string? ChangeDescription { get; private set; }
    public string AuthorId { get; private set; } = string.Empty;
    public DateTime? EffectiveDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public DocumentAttachment? Attachment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReleasedAt { get; private set; }
    public string? ReleasedBy { get; private set; }

    private DocumentVersion() { }

    internal static DocumentVersion Create(
        Guid id,
        TenantId tenantId,
        Guid documentMasterId,
        string versionNumber,
        string content,
        string authorId,
        string? changeDescription)
    {
        if (string.IsNullOrWhiteSpace(versionNumber))
            throw new DomainException("Version number is required.");

        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Document content is required.");

        return new DocumentVersion
        {
            Id = id,
            TenantId = tenantId,
            DocumentMasterId = documentMasterId,
            VersionNumber = versionNumber.Trim(),
            Status = DocumentStatus.Draft,
            Content = content,
            AuthorId = authorId,
            ChangeDescription = changeDescription?.Trim(),
            CreatedAt = DateTime.UtcNow,
        };
    }

    internal void SubmitForReview()
    {
        if (Status != DocumentStatus.Draft)
            throw new DomainException($"Cannot submit for review in status: {Status}.");

        Status = DocumentStatus.UnderReview;
    }

    internal void MoveToApproval()
    {
        if (Status != DocumentStatus.UnderReview)
            throw new DomainException($"Cannot move to approval in status: {Status}.");

        Status = DocumentStatus.PendingApproval;
    }

    internal void Release(string releasedBy, DateTime effectiveDate)
    {
        if (Status != DocumentStatus.PendingApproval)
            throw new DomainException($"Cannot release in status: {Status}.");

        Status = DocumentStatus.Released;
        ReleasedAt = DateTime.UtcNow;
        ReleasedBy = releasedBy;
        EffectiveDate = effectiveDate;
    }

    internal void Supersede()
    {
        if (Status != DocumentStatus.Released)
            throw new DomainException($"Cannot supersede version in status: {Status}.");

        Status = DocumentStatus.Superseded;
    }

    internal void MakeObsolete()
    {
        Status = DocumentStatus.Obsolete;
    }

    internal void SetAttachment(DocumentAttachment attachment)
    {
        Attachment = attachment ?? throw new DomainException("Attachment is required.");
    }

    internal void RevertToDraft()
    {
        if (Status != DocumentStatus.UnderReview && Status != DocumentStatus.PendingApproval)
            throw new DomainException($"Cannot revert to draft in status: {Status}.");

        Status = DocumentStatus.Draft;
    }
}
