using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.DocumentControl.Events;

public sealed record DocumentApprovedEvent(
    Guid DocumentId,
    string DocumentNumber,
    string VersionNumber,
    string ApprovedBy,
    DateTime EffectiveDate,
    Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
