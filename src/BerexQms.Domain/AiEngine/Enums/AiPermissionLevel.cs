namespace BerexQms.Domain.AiEngine.Enums;

/// <summary>
/// Defines the four tiered AI permission levels that govern what AI actions
/// a user may request. Higher levels unlock additional capabilities while
/// maintaining all restrictions from lower levels.
/// </summary>
public enum AiPermissionLevel
{
    /// <summary>
    /// Level 1 — AI Assistant (default for all authenticated users).
    /// Read-only AI actions: search, summarize, explain, draft generation.
    /// No database writes, no approvals, no workflow execution.
    /// </summary>
    Assistant = 1,

    /// <summary>
    /// Level 2 — AI Manager (manager-level permission).
    /// Adds: create draft workflows, assign draft tasks, prepare reports,
    /// generate management reviews, generate dashboards.
    /// Still prohibited: delete, approve, close CAPA/NCR, high-risk operations.
    /// </summary>
    Manager = 2,

    /// <summary>
    /// Level 3 — AI Administrator (system administrator).
    /// Adds: create records, update records, execute workflows, bulk notifications,
    /// generate KPI dashboards, schedule calibration, issue training assignments.
    /// High-risk operations still require confirmation.
    /// </summary>
    Administrator = 3,

    /// <summary>
    /// Level 4 — AI Super Administrator (JARVIS mode, Super Admin only).
    /// Full AI capabilities: database access, workflow execution, bulk operations,
    /// cross-module orchestration, AI automation, system diagnostics.
    /// Dangerous actions still require explicit confirmation.
    /// </summary>
    SuperAdministrator = 4,
}
