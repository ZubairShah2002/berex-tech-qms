using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.SupplierQuality.Events;

public sealed record SupplierScoreUpdatedEvent(
    Guid SupplierId,
    decimal NewScore,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    Guid TenantId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
