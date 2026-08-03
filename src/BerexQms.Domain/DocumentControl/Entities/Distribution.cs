using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.DocumentControl.Entities;

public sealed class Distribution : Entity<Guid>
{
    public Guid DocumentVersionId { get; private set; }
    public string RecipientId { get; private set; } = string.Empty;
    public DateTime DistributedAt { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }
    public DateTime ComplianceDeadline { get; private set; }
    public bool IsAcknowledged => AcknowledgedAt is not null;
    public bool IsOverdue => !IsAcknowledged && DateTime.UtcNow > ComplianceDeadline;

    private Distribution() { }

    internal static Distribution Create(
        Guid id,
        TenantId tenantId,
        Guid documentVersionId,
        string recipientId,
        DateTime complianceDeadline)
    {
        if (string.IsNullOrWhiteSpace(recipientId))
            throw new DomainException("Recipient ID is required.");

        return new Distribution
        {
            Id = id,
            TenantId = tenantId,
            DocumentVersionId = documentVersionId,
            RecipientId = recipientId,
            DistributedAt = DateTime.UtcNow,
            ComplianceDeadline = complianceDeadline,
        };
    }

    public void Acknowledge()
    {
        if (IsAcknowledged)
            throw new DomainException("Distribution has already been acknowledged.");

        AcknowledgedAt = DateTime.UtcNow;
    }
}
