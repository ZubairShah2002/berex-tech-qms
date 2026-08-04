using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.SupplierQuality.Commands.ReviewScarResponse;

public sealed record ReviewScarResponseCommand(
    Guid SupplierId,
    Guid ScarId,
    string Decision) : ICommand;
