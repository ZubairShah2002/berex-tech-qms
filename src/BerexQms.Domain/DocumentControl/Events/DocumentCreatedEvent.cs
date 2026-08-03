using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.DocumentControl.Events;

public sealed record DocumentCreatedEvent(
    Guid DocumentId,
    string DocumentNumber,
    string Title,
    string DocumentType,
    string OwnerId,
    Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
