using System.Text.Json;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetUserPreferences;

internal sealed class GetUserPreferencesQueryHandler
    : IQueryHandler<GetUserPreferencesQuery, AiUserPreferenceDto>
{
    private readonly IAiUserPreferenceRepository _preferenceRepo;
    private readonly ICurrentUserService _currentUser;

    public GetUserPreferencesQueryHandler(
        IAiUserPreferenceRepository preferenceRepo,
        ICurrentUserService currentUser)
    {
        _preferenceRepo = preferenceRepo;
        _currentUser = currentUser;
    }

    public async Task<Result<AiUserPreferenceDto>> Handle(
        GetUserPreferencesQuery request, CancellationToken ct)
    {
        var preference = await _preferenceRepo.GetByUserIdAsync(_currentUser.UserId, ct);

        if (preference == null)
        {
            // Return defaults — user has not configured preferences yet
            return Result<AiUserPreferenceDto>.Success(new AiUserPreferenceDto
            {
                AiEnabled = true,
                PreferredProvider = "Automatic",
                ConfirmationRequired = true,
                PreferredLanguage = null,
                EnabledTaskTypes = [],
            });
        }

        var dto = new AiUserPreferenceDto
        {
            AiEnabled = preference.IsAiEnabled,
            PreferredProvider = preference.PreferredProvider ?? "Automatic",
            ConfirmationRequired = preference.RequireConfirmationForRecommendations,
            PreferredLanguage = preference.PreferredLanguage,
            EnabledTaskTypes = ParseJsonList(preference.EnabledTaskTypesJson),
        };

        return Result<AiUserPreferenceDto>.Success(dto);
    }

    private static List<string> ParseJsonList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch { return []; }
    }
}
