using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Calibration.Events;

public sealed record EquipmentCalibratedEvent(
    Guid EquipmentId,
    string Result,
    Guid CalibrationRecordId,
    DateTime? NextDueDate,
    Guid TenantId) : DomainEvent;
