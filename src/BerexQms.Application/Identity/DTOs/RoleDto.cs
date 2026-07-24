namespace BerexQms.Application.Identity.DTOs;

public sealed record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsSystemRole,
    int PermissionCount);
