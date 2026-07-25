using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.ProductCatalog.Events;

public sealed record PartRevisionReleasedEvent(
    Guid PartId,
    Guid RevisionId,
    string RevisionCode,
    string PartNumber,
    Guid TenantId) : DomainEvent;
