using BerexQms.Application.AiEngine.DTOs;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Interfaces;

/// <summary>
/// AI governance service — resolves effective policy, validates access,
/// and enforces the layered governance model:
///
///   System Policy → Tenant Policy → Role/Permission → User Preference
///
/// A lower-level setting never overrides a higher-level restriction.
/// </summary>
public interface IAiGovernanceService
{
    /// <summary>
    /// Compute the effective AI policy for the current user.
    /// Merges governance policy, role permissions, and user preferences.
    /// </summary>
    Task<AiEffectivePolicyDto> GetEffectivePolicyAsync(
        Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Validate that the user is allowed to make an AI request for the given task.
    /// Returns success or a specific governance error.
    /// </summary>
    Task<Result> ValidateAiAccessAsync(
        Guid userId, string taskType, CancellationToken ct = default);

    /// <summary>Check if AI is enabled for the user (governance + preference).</summary>
    Task<bool> IsAiEnabledForUserAsync(
        Guid userId, CancellationToken ct = default);

    /// <summary>Check if a specific provider is allowed for the user.</summary>
    Task<bool> IsProviderAllowedAsync(
        Guid userId, string providerName, CancellationToken ct = default);

    /// <summary>Check if a specific task type is allowed for the user.</summary>
    Task<bool> IsTaskAllowedAsync(
        Guid userId, string taskType, CancellationToken ct = default);

    /// <summary>
    /// Resolve the effective preferred provider for the user.
    /// Returns null for Automatic (use orchestrator default routing).
    /// Returns a specific provider name only if it passes all governance checks.
    /// </summary>
    Task<string?> ResolvePreferredProviderAsync(
        Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Filter a provider routing chain based on governance policy.
    /// Removes providers that are not allowed by the tenant's governance policy.
    /// </summary>
    Task<List<string>> FilterAllowedProvidersAsync(
        List<string> providerChain, CancellationToken ct = default);
}
