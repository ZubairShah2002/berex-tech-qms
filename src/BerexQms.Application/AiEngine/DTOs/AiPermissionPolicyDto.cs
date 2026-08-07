namespace BerexQms.Application.AiEngine.DTOs;

public sealed record AiPermissionPolicyDto(
    Guid Id,
    Guid UserId,
    string PermissionLevel,
    bool IsActive,
    string? GrantedByUserId,
    DateTime GrantedAt,
    string? RevokedByUserId,
    DateTime? RevokedAt,
    string? Notes);
