using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.RevokeAiPermission;

public sealed record RevokeAiPermissionCommand(Guid UserId) : ICommand;
