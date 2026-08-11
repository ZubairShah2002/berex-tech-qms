namespace BerexQms.Application.AiEngine.DTOs;

/// <summary>
/// The effective AI policy for a user, computed by combining
/// system/tenant governance with the user's own preferences.
/// Read-only — represents the resolved state.
/// </summary>
public sealed class AiEffectivePolicyDto
{
    public bool AiEnabled { get; init; }
    public List<string> AllowedProviders { get; init; } = [];
    public string PreferredProvider { get; init; } = "Automatic";
    public List<string> AllowedTaskTypes { get; init; } = [];
    public bool ConfirmationRequired { get; init; } = true;
    public string? PreferredLanguage { get; init; }

    /// <summary>User's current daily request count.</summary>
    public int DailyRequestsUsed { get; init; }

    /// <summary>Daily limit (null = unlimited).</summary>
    public int? DailyRequestsLimit { get; init; }

    /// <summary>Tenant's current monthly request count.</summary>
    public int MonthlyRequestsUsed { get; init; }

    /// <summary>Monthly limit (null = unlimited).</summary>
    public int? MonthlyRequestsLimit { get; init; }
}
