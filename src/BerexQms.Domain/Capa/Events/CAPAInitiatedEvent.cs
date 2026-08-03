using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Capa.Events;

public sealed record CAPAInitiatedEvent(
    Guid CapaId,
    string CapaNumber,
    string SourceType,
    Guid? SourceNonConformanceId,
    string OwnerId,
    Guid TenantId) : DomainEvent;
