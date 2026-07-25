using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Inspection.Events;

public sealed record InspectionCreatedEvent(
    Guid InspectionId,
    string InspectionNumber,
    string Type,
    Guid PartId,
    Guid TenantId) : DomainEvent;
