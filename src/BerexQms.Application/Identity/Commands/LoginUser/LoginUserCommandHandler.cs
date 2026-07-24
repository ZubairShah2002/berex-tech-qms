using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;
using BerexQms.Application.Identity.Interfaces;
using BerexQms.Domain.Identity.Enums;
using BerexQms.Domain.Identity.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Identity.Commands.LoginUser;

public sealed class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, AuthTokenDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthTokenDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null)
            return UserErrors.InvalidCredentials;

        if (user.Status == UserStatus.Inactive)
            return UserErrors.AccountInactive;

        if (user.IsLockedOut)
            return UserErrors.AccountLocked;

        if (user.Status == UserStatus.Locked && !user.IsLockedOut)
            user.Unlock();

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);
            return UserErrors.InvalidCredentials;
        }

        user.RecordSuccessfulLogin();

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var accessToken = _tokenService.GenerateAccessToken(user, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshExpiry = DateTime.UtcNow.AddDays(7);

        user.SetRefreshToken(refreshToken, refreshExpiry);
        await _userRepository.UpdateAsync(user, cancellationToken);

        var userDto = new UserDto(
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

        return new AuthTokenDto(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(15),
            userDto);
    }
}
