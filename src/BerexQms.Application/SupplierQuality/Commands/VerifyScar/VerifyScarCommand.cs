using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.SupplierQuality.Commands.VerifyScar;

public sealed record VerifyScarCommand(
    Guid SupplierId,
    Guid ScarId,
    string Action) : ICommand;
