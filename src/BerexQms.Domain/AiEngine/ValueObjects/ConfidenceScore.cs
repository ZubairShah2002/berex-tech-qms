using BerexQms.Domain.AiEngine.Enums;
using BerexQms.SharedKernel.Exceptions;

namespace BerexQms.Domain.AiEngine.ValueObjects;

/// <summary>
/// Represents a confidence score (0.0-1.0) attached to an AI prediction or suggestion,
/// together with its derived confidence level and suppression status.
/// Thresholds: Low (0-0.30, suppressed), Moderate (0.31-0.60), High (0.61-0.85), Very High (0.86-1.0).
/// </summary>
public sealed record ConfidenceScore
{
    public decimal Score { get; }

    public ConfidenceLevel Level { get; }

    public bool IsSuppressed => Score <= 0.30m;

    private ConfidenceScore(decimal score, ConfidenceLevel level)
    {
        Score = score;
        Level = level;
    }

    public static ConfidenceScore Create(decimal score)
    {
        if (score < 0.0m || score > 1.0m)
            throw new DomainException("Confidence score must be between 0.0 and 1.0.");

        return new ConfidenceScore(score, ClassifyLevel(score));
    }

    private static ConfidenceLevel ClassifyLevel(decimal score) => score switch
    {
        <= 0.30m => ConfidenceLevel.Low,
        <= 0.60m => ConfidenceLevel.Moderate,
        <= 0.85m => ConfidenceLevel.High,
        _ => ConfidenceLevel.VeryHigh
    };
}
