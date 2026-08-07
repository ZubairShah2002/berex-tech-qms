namespace BerexQms.Domain.AiEngine.Enums;

/// <summary>
/// Tracks the confirmation status for dangerous AI actions that require
/// explicit user approval before execution.
/// </summary>
public enum ConfirmationStatus
{
    /// <summary>Action requires confirmation but has not yet been confirmed.</summary>
    Pending,

    /// <summary>User has explicitly confirmed the action — it may proceed.</summary>
    Confirmed,

    /// <summary>User has rejected the action — it will not execute.</summary>
    Rejected,

    /// <summary>Confirmation request expired without a response.</summary>
    Expired,
}
