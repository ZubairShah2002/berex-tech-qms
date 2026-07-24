using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Identity.Commands.AssignRole;

public sealed record AssignRoleCommand(Guid UserId, Guid RoleId) : ICommand;
