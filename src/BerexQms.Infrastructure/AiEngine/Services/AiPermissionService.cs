using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Domain.AiEngine;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;

namespace BerexQms.Infrastructure.AiEngine.Services;

/// <summary>
/// Resolves a user's effective AI permission level from the AiPermissionPolicy
/// table. Falls back to <see cref="AiPermissionLevel.Assistant"/> (Level 1)
/// when no explicit policy exists — every authenticated user gets basic
/// read-only AI capabilities by default.
/// </summary>
public sealed class AiPermissionService : IAiPermissionService
{
    private readonly IAiPermissionPolicyRepository _policyRepository;

    public AiPermissionService(IAiPermissionPolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<AiPermissionLevel> GetUserPermissionLevelAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var policy = await _policyRepository.GetActiveByUserAsync(userId, cancellationToken);

        if (policy is null)
            return AiPermissionLevel.Assistant; // Default for all authenticated users

        return Enum.TryParse<AiPermissionLevel>(policy.PermissionLevel, true, out var level)
            ? level
            : AiPermissionLevel.Assistant;
    }

    public async Task<bool> IsAuthorizedAsync(
        Guid userId, AiActionType actionType, CancellationToken cancellationToken = default)
    {
        var level = await GetUserPermissionLevelAsync(userId, cancellationToken);
        return AiActionPolicy.IsAuthorized(level, actionType);
    }
}
