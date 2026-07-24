using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Identity.DTOs;
using BerexQms.Application.Identity.Interfaces;
using BerexQms.Domain.Identity.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Identity.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, AuthTokenDto>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthTokenDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (user is null || user.RefreshTokenExpiryUtc < DateTime.UtcNow)
            return UserErrors.InvalidRefreshToken;

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
