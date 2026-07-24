using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;

namespace BerexQms.Application.Identity.Queries.ListRoles;

public sealed record ListRolesQuery : IQuery<IReadOnlyList<RoleDto>>;
