namespace BerexQms.Application.Interfaces;

/// <summary>
/// Service for recording audit log entries that capture entity-level
/// changes for compliance and traceability.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Records an audit log entry for an entity change.
    /// </summary>
    /// <param name="entityType">The type name of the entity being audited.</param>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <param name="action">The action performed (e.g., Created, Updated, Deleted).</param>
    /// <param name="oldValue">The serialized previous state of the entity, or <c>null</c> for creation.</param>
    /// <param name="newValue">The serialized new state of the entity, or <c>null</c> for deletion.</param>
    /// <param name="ct">Cancellation token.</param>
    Task LogAsync(string entityType, string entityId, string action, string? oldValue, string? newValue, CancellationToken ct);
}
