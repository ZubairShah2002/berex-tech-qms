using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;
using BerexQms.Application.Identity.Interfaces;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Identity.Entities;
using BerexQms.Domain.Identity.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Identity.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITenantContext _tenantContext;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        ITenantContext tenantContext)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _tenantContext = tenantContext;
    }

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
            return UserErrors.EmailAlreadyExists(request.Email);

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = User.Register(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.Email,
            request.FirstName,
            request.LastName,
            passwordHash,
            request.PhoneNumber,
            request.Department,
            request.JobTitle);

        if (request.RoleIds is { Count: > 0 })
        {
            var roles = await _roleRepository.GetByIdsAsync(request.RoleIds, cancellationToken);
            foreach (var role in roles)
            {
                user.AssignRole(role.Id, role.Name, "system");
            }
        }

        await _userRepository.AddAsync(user, cancellationToken);

        var roleNames = request.RoleIds is { Count: > 0 }
            ? user.UserRoles.Select(ur => ur.Role?.Name ?? "").Where(n => n != "").ToList()
            : new List<string>();

        return new UserDto(
            user.Id,
            user.Email.Value,
            user.Name.FirstName,
            user.Name.LastName,
            user.Name.DisplayName,
            user.Status.ToString(),
            user.PhoneNumber,
            user.Department,
            user.JobTitle,
            user.LastLoginAt,
            roleNames,
            user.CreatedAt);
    }
}
