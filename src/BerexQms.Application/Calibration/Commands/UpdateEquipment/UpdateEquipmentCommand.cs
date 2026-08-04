using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Calibration.Commands.UpdateEquipment;

public sealed record UpdateEquipmentCommand(
    Guid EquipmentId,
    string Name,
    string? Type,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    string? Location,
    string? Department,
    string? Area,
    Guid? CustodianId) : ICommand;
