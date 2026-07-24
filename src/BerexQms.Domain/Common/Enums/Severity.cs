namespace BerexQms.Domain.Common.Enums;

/// <summary>
/// Classifies the severity of a quality finding, non-conformance, or audit observation.
/// Severity drives escalation rules, notification routing, and regulatory reporting requirements.
/// </summary>
public enum Severity
{
    /// <summary>
    /// A defect or finding that could affect product safety, regulatory compliance,
    /// or results in a complete failure of a critical-to-quality characteristic.
    /// Requires immediate containment and management notification.
    /// </summary>
    Critical = 0,

    /// <summary>
    /// A significant departure from specification or procedure that impacts
    /// product quality or process capability but does not pose an immediate safety risk.
    /// Requires corrective action within a defined timeframe.
    /// </summary>
    Major = 1,

    /// <summary>
    /// A deviation that does not materially affect product quality or fitness for use.
    /// Tracked for trend analysis and continuous improvement but does not require
    /// immediate corrective action.
    /// </summary>
    Minor = 2
}
