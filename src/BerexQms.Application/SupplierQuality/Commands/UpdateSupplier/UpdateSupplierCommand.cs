using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.SupplierQuality.Commands.UpdateSupplier;

public sealed record UpdateSupplierCommand(
    Guid SupplierId,
    string Name,
    string? Tier,
    string? ContactName,
    string? ContactRole,
    string? ContactEmail,
    string? ContactPhone) : ICommand;
