using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.Identity.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Identity.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetCurrentUserQueryHandler(
        IUserRepository userRepository,
        ICurrentUserService currentUserService)
    {
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_currentUserService.UserId, cancellationToken);
        if (user is null)
            return UserErrors.UserNotFound(_currentUserService.UserId);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

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
            roles,
            user.CreatedAt);
    }
}
