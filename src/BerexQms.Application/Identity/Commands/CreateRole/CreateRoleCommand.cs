using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;

namespace BerexQms.Application.Identity.Commands.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    string? Description = null) : ICommand<RoleDto>;
