using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Inspection.Events;

public sealed record InspectionCompletedEvent(
    Guid InspectionId,
    string InspectionNumber,
    string Type,
    string Result,
    Guid PartId,
    int TotalMeasurements,
    int FailedMeasurements,
    Guid TenantId) : DomainEvent;
