namespace BerexQms.Domain.Common.Enums;

/// <summary>
/// Indicates the urgency of a quality action, task, or issue.
/// Priority determines response time requirements, escalation paths,
/// and resource allocation decisions.
/// </summary>
public enum Priority
{
    /// <summary>
    /// No immediate urgency. The item can be addressed within the standard
    /// resolution timeframe as resources permit.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Moderate urgency. The item should be addressed within the standard
    /// resolution timeframe and tracked through regular status reviews.
    /// </summary>
    Medium = 1,

    /// <summary>
    /// Elevated urgency. The item requires prompt attention, expedited review cycles,
    /// and proactive status updates to stakeholders.
    /// </summary>
    High = 2,

    /// <summary>
    /// Highest urgency. The item demands immediate action, management notification,
    /// and potentially production hold or customer containment activities.
    /// Typically associated with safety-related or regulatory non-conformances.
    /// </summary>
    Critical = 3
}
