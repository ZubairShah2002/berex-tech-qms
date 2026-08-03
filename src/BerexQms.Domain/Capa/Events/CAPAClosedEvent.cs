using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Capa.Events;

public sealed record CAPAClosedEvent(
    Guid CapaId,
    string CapaNumber,
    string ClosureStatus,
    Guid TenantId) : DomainEvent;
