using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.DocumentControl.Events;

public sealed record VersionReleasedEvent(
    Guid DocumentId,
    string DocumentNumber,
    string VersionNumber,
    string ReleasedBy,
    DateTime EffectiveDate,
    Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
