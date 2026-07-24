namespace BerexQms.Application.Identity.DTOs;

public sealed record AuthTokenDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User);
