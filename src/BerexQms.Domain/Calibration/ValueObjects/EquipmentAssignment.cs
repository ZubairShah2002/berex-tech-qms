namespace BerexQms.Domain.Calibration.ValueObjects;

public sealed record EquipmentAssignment(
    string Department,
    string? Area,
    Guid? CustodianId);
