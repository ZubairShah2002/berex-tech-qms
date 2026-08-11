namespace BerexQms.Application.AiEngine.DTOs;

/// <summary>
/// Tenant-level AI governance policy for administrator management.
/// </summary>
public sealed class AiGovernancePolicyDto
{
    public bool AiEnabled { get; init; } = true;
    public List<string> AllowedProviders { get; init; } = [];
    public string? DefaultProvider { get; init; }
    public List<string> AllowedTaskTypes { get; init; } = [];
    public int? MaxDailyRequestsPerUser { get; init; }
    public int? MaxMonthlyRequestsPerTenant { get; init; }
    public bool RequireHumanConfirmation { get; init; } = true;
}
