namespace BerexQms.Domain.Calibration.Enums;

public enum EquipmentStatus
{
    Active = 0,
    DueForCalibration = 1,
    Overdue = 2,
    OutOfService = 3,
    InCalibration = 4,
    Retired = 5
}
