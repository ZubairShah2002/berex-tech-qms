namespace BerexQms.Application.Identity.DTOs;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string DisplayName,
    string Status,
    string? PhoneNumber,
    string? Department,
    string? JobTitle,
    DateTime? LastLoginAt,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt);
