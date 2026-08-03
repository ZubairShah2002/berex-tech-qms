using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Capa.Commands.AssignCapa;

public sealed record AssignCapaCommand(
    Guid CapaId,
    string AssigneeId) : ICommand;
