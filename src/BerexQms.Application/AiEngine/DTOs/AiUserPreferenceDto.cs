namespace BerexQms.Application.AiEngine.DTOs;

/// <summary>
/// User's AI preference settings.
/// </summary>
public sealed class AiUserPreferenceDto
{
    public bool AiEnabled { get; init; }
    public string PreferredProvider { get; init; } = "Automatic";
    public bool ConfirmationRequired { get; init; } = true;
    public string? PreferredLanguage { get; init; }
    public List<string> EnabledTaskTypes { get; init; } = [];
}
