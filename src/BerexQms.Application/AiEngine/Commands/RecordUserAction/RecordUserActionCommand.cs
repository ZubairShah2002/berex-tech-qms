using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.RecordUserAction;

public sealed record RecordUserActionCommand(
    Guid InteractionId,
    string Action,
    string? Justification) : ICommand;
