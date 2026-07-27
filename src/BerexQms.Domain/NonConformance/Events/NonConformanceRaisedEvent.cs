using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.NonConformance.Events;

public sealed record NonConformanceRaisedEvent(
    Guid NonConformanceId,
    string NcrNumber,
    string Severity,
    Guid PartId,
    string? DefectType,
    Guid TenantId) : DomainEvent;
