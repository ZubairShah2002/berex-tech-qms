namespace BerexQms.Domain.AiEngine.Enums;

/// <summary>
/// Categorises AI actions by their impact level to enforce permission checks
/// and mandatory confirmation requirements.
/// </summary>
public enum AiActionCategory
{
    /// <summary>
    /// Read-only operations: search, query, explain, summarize.
    /// Available to all authenticated users (Level 1+).
    /// </summary>
    ReadOnly,

    /// <summary>
    /// Draft/generation operations: generate CAPA draft, NCR draft, emails, reports.
    /// Available to all authenticated users (Level 1+) — outputs are recommendations only.
    /// </summary>
    Generate,

    /// <summary>
    /// Draft workflow operations: create draft workflows, assign draft tasks, prepare reports.
    /// Requires Level 2 (Manager) or higher.
    /// </summary>
    DraftWorkflow,

    /// <summary>
    /// Write operations: create records, update records, execute workflows.
    /// Requires Level 3 (Administrator) or higher.
    /// </summary>
    Write,

    /// <summary>
    /// Bulk operations: bulk notifications, bulk updates.
    /// Requires Level 4 (Super Administrator).
    /// </summary>
    BulkOperation,

    /// <summary>
    /// Cross-module orchestration: management review generation, cross-module analysis.
    /// Requires Level 4 (Super Administrator).
    /// </summary>
    CrossModuleOrchestration,

    /// <summary>
    /// Dangerous operations that always require explicit confirmation regardless
    /// of permission level: delete, archive, deactivate, approve, reject,
    /// close CAPA/NCR/Audit/SCAR, bulk delete, configuration changes,
    /// user management, permission changes, AI model changes.
    /// </summary>
    Dangerous,
}
