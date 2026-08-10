namespace BerexQms.Domain.AiEngine.Enums;

/// <summary>
/// Indicates the severity level of an AI recommendation.
/// </summary>
public enum AiSeverity
{
    /// <summary>Informational finding — no immediate action required.</summary>
    Low = 1,

    /// <summary>Moderate finding — should be reviewed and addressed in normal workflow.</summary>
    Medium = 2,

    /// <summary>High severity — prompt attention recommended.</summary>
    High = 3,

    /// <summary>Critical finding — immediate attention required to prevent quality failure.</summary>
    Critical = 4,
}
