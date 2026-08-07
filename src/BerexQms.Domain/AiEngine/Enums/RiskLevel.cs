namespace BerexQms.Domain.AiEngine.Enums;

/// <summary>
/// Risk classification for AI actions, used in the mandatory confirmation
/// layer to communicate the impact of an action to the user.
/// </summary>
public enum RiskLevel
{
    /// <summary>No risk — read-only or informational actions.</summary>
    None,

    /// <summary>Low risk — draft/generation actions with no side effects.</summary>
    Low,

    /// <summary>Medium risk — write operations on individual records.</summary>
    Medium,

    /// <summary>High risk — bulk operations, cross-module changes.</summary>
    High,

    /// <summary>Critical risk — delete, deactivate, permission changes, configuration.</summary>
    Critical,
}
