using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.AssignAiPermissionLevel;

public sealed record AssignAiPermissionLevelCommand(
    Guid UserId,
    string PermissionLevel,
    string? Notes) : ICommand;
