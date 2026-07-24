using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Common.Events;

/// <summary>
/// Raised when a calibration activity is completed for a piece of equipment.
/// Consumed by the Inspection module (to re-enable integrity gate checks for the equipment)
/// and the Notification module (to confirm calibration completion to stakeholders).
/// </summary>
public sealed record EquipmentCalibratedEvent(
    Guid EquipmentId,
    string Result,
    Guid CertificateId,
    DateTime NextDueDate) : DomainEvent;
