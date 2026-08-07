using BerexQms.Domain.AiEngine.Enums;

namespace BerexQms.Application.AiEngine.Interfaces;

/// <summary>
/// Resolves the AI permission level for a user within a tenant.
/// Implemented in Infrastructure — resolves from the AiPermissionPolicy
/// table with a fallback to Level 1 (Assistant) when no policy exists.
/// </summary>
public interface IAiPermissionService
{
    /// <summary>
    /// Resolves the effective <see cref="AiPermissionLevel"/> for the given user.
    /// Returns <see cref="AiPermissionLevel.Assistant"/> if no explicit policy exists.
    /// </summary>
    Task<AiPermissionLevel> GetUserPermissionLevelAsync(
        Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the user is authorised to perform the specified action type.
    /// </summary>
    Task<bool> IsAuthorizedAsync(
        Guid userId, AiActionType actionType, CancellationToken cancellationToken = default);
}
