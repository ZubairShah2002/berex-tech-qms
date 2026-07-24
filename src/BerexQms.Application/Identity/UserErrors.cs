using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Identity;

public static class UserErrors
{
    public static readonly Error InvalidCredentials = Error.Unauthorized(
        "Auth.InvalidCredentials", "Invalid email or password.");

    public static readonly Error AccountLocked = Error.Unauthorized(
        "Auth.AccountLocked", "Account is locked due to too many failed login attempts.");

    public static readonly Error AccountInactive = Error.Unauthorized(
        "Auth.AccountInactive", "Account is inactive.");

    public static readonly Error InvalidRefreshToken = Error.Unauthorized(
        "Auth.InvalidRefreshToken", "Refresh token is invalid or expired.");

    public static Error EmailAlreadyExists(string email) => Error.Conflict(
        "User.EmailExists", $"A user with email '{email}' already exists.");

    public static Error UserNotFound(Guid id) => Error.NotFound(
        "User.NotFound", $"User with ID '{id}' was not found.");

    public static Error RoleNotFound(Guid id) => Error.NotFound(
        "Role.NotFound", $"Role with ID '{id}' was not found.");

    public static Error RoleNotFoundByName(string name) => Error.NotFound(
        "Role.NotFound", $"Role '{name}' was not found.");

    public static Error RoleNameExists(string name) => Error.Conflict(
        "Role.NameExists", $"A role named '{name}' already exists.");

    public static Error TenantNotFound(Guid id) => Error.NotFound(
        "Tenant.NotFound", $"Tenant with ID '{id}' was not found.");

    public static Error TenantCodeExists(string code) => Error.Conflict(
        "Tenant.CodeExists", $"A tenant with code '{code}' already exists.");
}
