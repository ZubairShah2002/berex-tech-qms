namespace BerexQms.Domain.AiEngine.Enums;

/// <summary>
/// Category of an AI assertion. AI must clearly distinguish between facts,
/// inferences, recommendations, and unknowns — never present assumptions
/// as confirmed facts.
/// </summary>
public enum AiConfidenceCategory
{
    /// <summary>Verifiable from provided evidence.</summary>
    Fact = 1,

    /// <summary>Derived from evidence but not directly confirmed.</summary>
    Inference = 2,

    /// <summary>Suggested action based on analysis.</summary>
    Recommendation = 3,

    /// <summary>Cannot be determined from available data.</summary>
    Unknown = 4,
}
