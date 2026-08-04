using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.SupplierQuality.Commands.RespondToScar;

public sealed record RespondToScarCommand(
    Guid SupplierId,
    Guid ScarId,
    string RootCause,
    string CorrectiveActions,
    string? EvidenceRefs) : ICommand;
