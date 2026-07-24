using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Common.Events;

/// <summary>
/// Raised when a piece of calibrated equipment is approaching or has reached its calibration due date.
/// Consumed by the Notification module (to alert calibration technicians and supervisors)
/// and the Inspection module (to flag integrity gate failures for inspections
/// using equipment with lapsed calibration).
/// </summary>
public sealed record CalibrationDueEvent(
    Guid EquipmentId,
    DateTime DueDate,
    DateTime? LastCalibrationDate) : DomainEvent;
