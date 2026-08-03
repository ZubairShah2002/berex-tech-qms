using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Capa.Commands.CompleteAction;

public sealed record CompleteActionCommand(
    Guid CapaId,
    Guid ActionId,
    string? CompletionNotes,
    string? EvidenceProvided) : ICommand;
