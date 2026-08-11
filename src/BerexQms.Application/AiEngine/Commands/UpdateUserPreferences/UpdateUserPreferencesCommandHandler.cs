using System.Text.Json;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel;
using BerexQms.SharedKernel.Results;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Application.AiEngine.Commands.UpdateUserPreferences;

internal sealed class UpdateUserPreferencesCommandHandler
    : ICommandHandler<UpdateUserPreferencesCommand, AiUserPreferenceDto>
{
    private readonly IAiUserPreferenceRepository _preferenceRepo;
    private readonly IAiGovernancePolicyRepository _governanceRepo;
    private readonly ICurrentUserService _currentUser;

    public UpdateUserPreferencesCommandHandler(
        IAiUserPreferenceRepository preferenceRepo,
        IAiGovernancePolicyRepository governanceRepo,
        ICurrentUserService currentUser)
    {
        _preferenceRepo = preferenceRepo;
        _governanceRepo = governanceRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<AiUserPreferenceDto>> Handle(
        UpdateUserPreferencesCommand request, CancellationToken ct)
    {
        var userId = _currentUser.UserId;
        var tenantId = TenantId.From(_currentUser.TenantId);

        // Validate preferred provider against governance
        if (!string.IsNullOrEmpty(request.PreferredProvider)
            && request.PreferredProvider != "Automatic")
        {
            var governance = await _governanceRepo.GetByTenantAsync(ct);
            if (governance != null)
            {
                var allowedProviders = ParseJsonList(governance.AllowedProvidersJson);
                if (allowedProviders.Count > 0
                    && !allowedProviders.Contains(request.PreferredProvider, StringComparer.OrdinalIgnoreCase))
                {
                    return Result<AiUserPreferenceDto>.Failure(
                        AiGovernanceErrors.ProviderNotPermitted(request.PreferredProvider));
                }
            }
        }

        // Validate task types against governance
        if (request.EnabledTaskTypes is { Count: > 0 })
        {
            var governance = await _governanceRepo.GetByTenantAsync(ct);
            if (governance != null)
            {
                var allowedTasks = ParseJsonList(governance.AllowedTaskTypesJson);
                if (allowedTasks.Count > 0)
                {
                    var invalidTasks = request.EnabledTaskTypes
                        .Where(t => !allowedTasks.Contains(t, StringComparer.OrdinalIgnoreCase))
                        .ToList();

                    if (invalidTasks.Count > 0)
                    {
                        return Result<AiUserPreferenceDto>.Failure(
                            AiGovernanceErrors.TaskNotPermitted(invalidTasks[0]));
                    }
                }
            }
        }

        // Get or create preference
        var preference = await _preferenceRepo.GetByUserIdAsync(userId, ct);

        if (preference == null)
        {
            preference = AiUserPreference.Create(Guid.NewGuid(), tenantId, userId);
            await _preferenceRepo.AddAsync(preference, ct);
        }

        // Apply changes
        preference.SetAiEnabled(request.AiEnabled);
        preference.SetPreferredProvider(
            request.PreferredProvider == "Automatic" ? null : request.PreferredProvider);
        preference.SetConfirmationPreference(request.ConfirmationRequired);
        preference.SetPreferredLanguage(request.PreferredLanguage);
        preference.SetEnabledTaskTypes(
            request.EnabledTaskTypes is { Count: > 0 }
                ? JsonSerializer.Serialize(request.EnabledTaskTypes)
                : null);

        await _preferenceRepo.UpdateAsync(preference, ct);

        return Result<AiUserPreferenceDto>.Success(MapToDto(preference));
    }

    private static AiUserPreferenceDto MapToDto(AiUserPreference pref) => new()
    {
        AiEnabled = pref.IsAiEnabled,
        PreferredProvider = pref.PreferredProvider ?? "Automatic",
        ConfirmationRequired = pref.RequireConfirmationForRecommendations,
        PreferredLanguage = pref.PreferredLanguage,
        EnabledTaskTypes = ParseJsonList(pref.EnabledTaskTypesJson),
    };

    private static List<string> ParseJsonList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch { return []; }
    }
}
