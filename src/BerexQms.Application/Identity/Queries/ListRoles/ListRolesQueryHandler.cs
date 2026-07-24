using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;
using BerexQms.Domain.Identity.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Identity.Queries.ListRoles;

public sealed class ListRolesQueryHandler : IQueryHandler<ListRolesQuery, IReadOnlyList<RoleDto>>
{
    private readonly IRoleRepository _roleRepository;

    public ListRolesQueryHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<IReadOnlyList<RoleDto>>> Handle(ListRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.ListAllAsync(cancellationToken);

        IReadOnlyList<RoleDto> dtos = roles.Select(r => new RoleDto(
            r.Id,
            r.Name,
            r.Description,
            r.IsSystemRole,
            r.RolePermissions.Count)).ToList();

        return Result.Success(dtos);
    }
}
