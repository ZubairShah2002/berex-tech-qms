using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Calibration.Commands.RegisterEquipment;

public sealed record RegisterEquipmentCommand(
    string Code,
    string Name,
    string? Type,
    string? Manufacturer,
    string? Model,
    string? SerialNumber,
    string? Location,
    string? Department,
    string? Area,
    Guid? CustodianId) : ICommand<Guid>;
