using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.ProductCatalog.Events;

public sealed record PartCreatedEvent(
    Guid PartId,
    string PartNumber,
    string Name,
    Guid TenantId) : DomainEvent;
