using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.ProductCatalog.Events;

public sealed record PartObsoletedEvent(
    Guid PartId,
    string PartNumber,
    Guid TenantId) : DomainEvent;
