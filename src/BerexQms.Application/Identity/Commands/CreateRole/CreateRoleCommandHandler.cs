using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Identity.Entities;
using BerexQms.Domain.Identity.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Identity.Commands.CreateRole;

public sealed class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, RoleDto>
{
    private readonly IRoleRepository _roleRepository;
    private readonly ITenantContext _tenantContext;

    public CreateRoleCommandHandler(
        IRoleRepository roleRepository,
        ITenantContext tenantContext)
    {
        _roleRepository = roleRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (await _roleRepository.NameExistsAsync(request.Name, cancellationToken))
            return UserErrors.RoleNameExists(request.Name);

        var role = Role.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.Name,
            request.Description);

        await _roleRepository.AddAsync(role, cancellationToken);

        return new RoleDto(role.Id, role.Name, role.Description, role.IsSystemRole, 0);
    }
}
