namespace BerexQms.Application.Calibration.DTOs;

public sealed record EquipmentDto(
    Guid Id,
    string Code,
    string Name,
    string? Type,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    string Status,
    string? Location,
    string? Department,
    DateTime? NextDueDate,
    int CalibrationCount,
    DateTime CreatedAt);
