using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AuditManagement.Events;

public sealed record AuditFindingRecordedEvent(
    Guid FindingId,
    Guid AuditId,
    string FindingClassification,
    string Area,
    Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
