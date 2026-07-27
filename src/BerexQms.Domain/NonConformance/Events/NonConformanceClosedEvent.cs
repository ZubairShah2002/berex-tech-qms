using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.NonConformance.Events;

public sealed record NonConformanceClosedEvent(
    Guid NonConformanceId,
    string NcrNumber,
    string DispositionType,
    Guid TenantId) : DomainEvent;
