using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.SupplierQuality.Commands.CreateSupplier;

public sealed record CreateSupplierCommand(
    string Code,
    string Name,
    string? Tier,
    string? ContactName,
    string? ContactRole,
    string? ContactEmail,
    string? ContactPhone) : ICommand<Guid>;
