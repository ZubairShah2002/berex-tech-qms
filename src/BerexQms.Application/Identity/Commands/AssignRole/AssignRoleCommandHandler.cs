using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Identity.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Identity.Commands.AssignRole;

public sealed class AssignRoleCommandHandler : ICommandHandler<AssignRoleCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICurrentUserService _currentUserService;

    public AssignRoleCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound(request.UserId));

        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
            return Result.Failure(UserErrors.RoleNotFound(request.RoleId));

        user.AssignRole(role.Id, role.Name, _currentUserService.UserId.ToString());
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result.Success();
    }
}
