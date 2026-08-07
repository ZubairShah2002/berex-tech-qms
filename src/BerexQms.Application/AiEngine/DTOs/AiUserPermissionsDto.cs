namespace BerexQms.Application.AiEngine.DTOs;

/// <summary>
/// Summary of a user's effective AI permissions, including the list of
/// actions they are authorized to perform at their current permission level.
/// </summary>
public sealed record AiUserPermissionsDto(
    Guid UserId,
    string PermissionLevel,
    int PermissionLevelNumber,
    string[] AllowedActionTypes,
    string[] AllowedCategories,
    bool HasExplicitPolicy);
