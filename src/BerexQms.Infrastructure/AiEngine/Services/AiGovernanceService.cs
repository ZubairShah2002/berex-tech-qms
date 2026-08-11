using System.Text.Json;
using BerexQms.Application.AiEngine;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace BerexQms.Infrastructure.AiEngine.Services;

/// <summary>
/// AI governance service — enforces the layered governance model:
///
///   System Policy → Tenant Policy → Role/Permission → User Preference
///
/// Each layer can only restrict, never widen, what the layer above allows.
///
/// SECURITY: Never logs sensitive policy details, prompts, or AI responses.
/// Logs only governance decisions using structured metadata.
/// </summary>
internal sealed class AiGovernanceService : IAiGovernanceService
{
    private readonly IAiGovernancePolicyRepository _policyRepo;
    private readonly IAiUserPreferenceRepository _preferenceRepo;
    private readonly IAiUsageRecordRepository _usageRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<AiGovernanceService> _logger;

    public AiGovernanceService(
        IAiGovernancePolicyRepository policyRepo,
        IAiUserPreferenceRepository preferenceRepo,
        IAiUsageRecordRepository usageRepo,
        ICurrentUserService currentUser,
        ILogger<AiGovernanceService> logger)
    {
        _policyRepo = policyRepo;
        _preferenceRepo = preferenceRepo;
        _usageRepo = usageRepo;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<AiEffectivePolicyDto> GetEffectivePolicyAsync(
        Guid userId, CancellationToken ct)
    {
        var governance = await _policyRepo.GetByTenantAsync(ct);
        var preference = await _preferenceRepo.GetByUserIdAsync(userId, ct);

        // Resolve AI enabled: governance AND user preference
        var governanceEnabled = governance?.IsAiEnabled ?? true;
        var userEnabled = preference?.IsAiEnabled ?? true;
        var aiEnabled = governanceEnabled && userEnabled;

        // Resolve allowed providers: governance restrictions
        var allowedProviders = ResolveAllowedProviders(governance);

        // Resolve preferred provider: user preference, validated against governance
        var preferredProvider = ResolvePreferredProvider(preference, allowedProviders);

        // Resolve allowed tasks: intersection of governance and user preferences
        var allowedTasks = ResolveAllowedTasks(governance, preference);

        // Confirmation always required for QMS modifications
        var confirmationRequired = true;

        // Get usage counts
        var dailyCount = await _usageRepo.CountUserRequestsTodayAsync(userId, ct);
        var monthlyCount = await _usageRepo.CountTenantRequestsThisMonthAsync(
            _currentUser.TenantId, ct);

        return new AiEffectivePolicyDto
        {
            AiEnabled = aiEnabled,
            AllowedProviders = allowedProviders,
            PreferredProvider = preferredProvider,
            AllowedTaskTypes = allowedTasks,
            ConfirmationRequired = confirmationRequired,
            PreferredLanguage = preference?.PreferredLanguage,
            DailyRequestsUsed = dailyCount,
            DailyRequestsLimit = governance?.MaxDailyRequestsPerUser,
            MonthlyRequestsUsed = monthlyCount,
            MonthlyRequestsLimit = governance?.MaxMonthlyRequestsPerTenant,
        };
    }

    public async Task<Result> ValidateAiAccessAsync(
        Guid userId, string taskType, CancellationToken ct)
    {
        var governance = await _policyRepo.GetByTenantAsync(ct);
        var preference = await _preferenceRepo.GetByUserIdAsync(userId, ct);

        // 1. Check governance-level AI enabled
        if (governance is { IsAiEnabled: false })
        {
            _logger.LogWarning(
                "AI request blocked: AI disabled by governance policy for tenant {TenantId}",
                _currentUser.TenantId);
            return Result.Failure(AiGovernanceErrors.AiDisabledByPolicy);
        }

        // 2. Check user-level AI enabled
        if (preference is { IsAiEnabled: false })
        {
            _logger.LogInformation(
                "AI request blocked: AI disabled by user preference for user {UserId}",
                userId);
            return Result.Failure(AiGovernanceErrors.AiDisabled);
        }

        // 3. Check task permission
        if (!IsTaskAllowedInternal(governance, preference, taskType))
        {
            _logger.LogWarning(
                "AI request blocked: task {TaskType} not permitted for user {UserId}",
                taskType, userId);
            return Result.Failure(AiGovernanceErrors.TaskNotPermitted(taskType));
        }

        // 4. Check usage limits
        if (governance?.MaxDailyRequestsPerUser != null)
        {
            var dailyCount = await _usageRepo.CountUserRequestsTodayAsync(userId, ct);
            if (dailyCount >= governance.MaxDailyRequestsPerUser.Value)
            {
                _logger.LogWarning(
                    "AI request blocked: daily limit reached for user {UserId} ({Count}/{Limit})",
                    userId, dailyCount, governance.MaxDailyRequestsPerUser);
                return Result.Failure(AiGovernanceErrors.DailyLimitReached);
            }
        }

        if (governance?.MaxMonthlyRequestsPerTenant != null)
        {
            var monthlyCount = await _usageRepo.CountTenantRequestsThisMonthAsync(
                _currentUser.TenantId, ct);
            if (monthlyCount >= governance.MaxMonthlyRequestsPerTenant.Value)
            {
                _logger.LogWarning(
                    "AI request blocked: monthly tenant limit reached ({Count}/{Limit})",
                    monthlyCount, governance.MaxMonthlyRequestsPerTenant);
                return Result.Failure(AiGovernanceErrors.MonthlyLimitReached);
            }
        }

        return Result.Success();
    }

    public async Task<bool> IsAiEnabledForUserAsync(Guid userId, CancellationToken ct)
    {
        var governance = await _policyRepo.GetByTenantAsync(ct);
        if (governance is { IsAiEnabled: false })
            return false;

        var preference = await _preferenceRepo.GetByUserIdAsync(userId, ct);
        return preference?.IsAiEnabled ?? true;
    }

    public async Task<bool> IsProviderAllowedAsync(
        Guid userId, string providerName, CancellationToken ct)
    {
        var governance = await _policyRepo.GetByTenantAsync(ct);
        var allowedProviders = ResolveAllowedProviders(governance);

        // Empty list means all are allowed
        if (allowedProviders.Count == 0)
            return true;

        return allowedProviders.Contains(providerName, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<bool> IsTaskAllowedAsync(
        Guid userId, string taskType, CancellationToken ct)
    {
        var governance = await _policyRepo.GetByTenantAsync(ct);
        var preference = await _preferenceRepo.GetByUserIdAsync(userId, ct);
        return IsTaskAllowedInternal(governance, preference, taskType);
    }

    public async Task<string?> ResolvePreferredProviderAsync(
        Guid userId, CancellationToken ct)
    {
        var governance = await _policyRepo.GetByTenantAsync(ct);
        var preference = await _preferenceRepo.GetByUserIdAsync(userId, ct);

        var allowedProviders = ResolveAllowedProviders(governance);
        return ResolvePreferredProvider(preference, allowedProviders) is "Automatic"
            ? null
            : ResolvePreferredProvider(preference, allowedProviders);
    }

    public async Task<List<string>> FilterAllowedProvidersAsync(
        List<string> providerChain, CancellationToken ct)
    {
        var governance = await _policyRepo.GetByTenantAsync(ct);
        var allowedProviders = ResolveAllowedProviders(governance);

        // No governance restrictions — allow the full chain
        if (allowedProviders.Count == 0)
            return providerChain;

        return providerChain
            .Where(p => allowedProviders.Contains(p, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }

    // ---- Private helpers ----

    private static List<string> ResolveAllowedProviders(
        Domain.AiEngine.Entities.AiGovernancePolicy? governance)
    {
        if (governance == null)
            return []; // No governance = all providers allowed

        return ParseJsonList(governance.AllowedProvidersJson);
    }

    private static string ResolvePreferredProvider(
        Domain.AiEngine.Entities.AiUserPreference? preference,
        List<string> allowedProviders)
    {
        var userPref = preference?.PreferredProvider;

        if (string.IsNullOrEmpty(userPref))
            return "Automatic";

        // Validate user preference against governance
        if (allowedProviders.Count > 0
            && !allowedProviders.Contains(userPref, StringComparer.OrdinalIgnoreCase))
        {
            // User's preferred provider is not allowed — fall back to Automatic
            return "Automatic";
        }

        return userPref;
    }

    private static List<string> ResolveAllowedTasks(
        Domain.AiEngine.Entities.AiGovernancePolicy? governance,
        Domain.AiEngine.Entities.AiUserPreference? preference)
    {
        var governanceTasks = governance != null
            ? ParseJsonList(governance.AllowedTaskTypesJson)
            : [];

        var userTasks = preference != null
            ? ParseJsonList(preference.EnabledTaskTypesJson)
            : [];

        // If governance has no restrictions and user has no preferences → empty (all allowed)
        if (governanceTasks.Count == 0 && userTasks.Count == 0)
            return [];

        // If only governance has restrictions → return those
        if (userTasks.Count == 0)
            return governanceTasks;

        // If only user has preferences → return those
        if (governanceTasks.Count == 0)
            return userTasks;

        // Intersection: governance-allowed ∩ user-enabled
        return userTasks
            .Where(t => governanceTasks.Contains(t, StringComparer.OrdinalIgnoreCase))
            .ToList();
    }

    private static bool IsTaskAllowedInternal(
        Domain.AiEngine.Entities.AiGovernancePolicy? governance,
        Domain.AiEngine.Entities.AiUserPreference? preference,
        string taskType)
    {
        // Check governance restrictions
        if (governance != null)
        {
            var governanceTasks = ParseJsonList(governance.AllowedTaskTypesJson);
            if (governanceTasks.Count > 0
                && !governanceTasks.Contains(taskType, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        // Check user preferences
        if (preference != null)
        {
            var userTasks = ParseJsonList(preference.EnabledTaskTypesJson);
            if (userTasks.Count > 0
                && !userTasks.Contains(taskType, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static List<string> ParseJsonList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch { return []; }
    }
}
