using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.Identity.Commands.RemoveRole;

public sealed record RemoveRoleCommand(Guid UserId, Guid RoleId) : ICommand;
