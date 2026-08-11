using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.UpdateUserPreferences;

/// <summary>
/// Update the current user's AI preferences.
/// Users can enable/disable AI, set preferred provider, and configure task preferences.
/// All changes are validated against governance policy before persisting.
/// </summary>
public sealed record UpdateUserPreferencesCommand(
    bool AiEnabled,
    string? PreferredProvider,
    bool ConfirmationRequired,
    string? PreferredLanguage,
    List<string>? EnabledTaskTypes) : ICommand<AiUserPreferenceDto>;
