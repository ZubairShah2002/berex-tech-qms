using BerexQms.Domain.AiEngine.Enums;

namespace BerexQms.Domain.AiEngine;

/// <summary>
/// Pure domain logic that maps each <see cref="AiActionType"/> to its
/// <see cref="AiActionCategory"/>, minimum <see cref="AiPermissionLevel"/>,
/// <see cref="RiskLevel"/>, and whether explicit confirmation is mandatory.
/// No infrastructure dependencies — this is the single source of truth for
/// AI permission rules.
/// </summary>
public static class AiActionPolicy
{
    private sealed record ActionRule(
        AiActionCategory Category,
        AiPermissionLevel MinimumLevel,
        RiskLevel Risk,
        bool RequiresConfirmation);

    private static readonly Dictionary<AiActionType, ActionRule> Rules = new()
    {
        // ---- Level 1: AI Assistant (ReadOnly / Generate) ----
        [AiActionType.ReadData]                = new(AiActionCategory.ReadOnly, AiPermissionLevel.Assistant, RiskLevel.None, false),
        [AiActionType.SearchDocuments]         = new(AiActionCategory.ReadOnly, AiPermissionLevel.Assistant, RiskLevel.None, false),
        [AiActionType.ExplainSop]              = new(AiActionCategory.ReadOnly, AiPermissionLevel.Assistant, RiskLevel.None, false),
        [AiActionType.ExplainProcedure]        = new(AiActionCategory.ReadOnly, AiPermissionLevel.Assistant, RiskLevel.None, false),
        [AiActionType.SummarizeReport]         = new(AiActionCategory.ReadOnly, AiPermissionLevel.Assistant, RiskLevel.None, false),
        [AiActionType.AnswerQuestion]          = new(AiActionCategory.ReadOnly, AiPermissionLevel.Assistant, RiskLevel.None, false),
        [AiActionType.GenerateEmail]           = new(AiActionCategory.Generate, AiPermissionLevel.Assistant, RiskLevel.Low, false),
        [AiActionType.GenerateCapaDraft]       = new(AiActionCategory.Generate, AiPermissionLevel.Assistant, RiskLevel.Low, false),
        [AiActionType.GenerateNcrDraft]        = new(AiActionCategory.Generate, AiPermissionLevel.Assistant, RiskLevel.Low, false),
        [AiActionType.GenerateAuditDraft]      = new(AiActionCategory.Generate, AiPermissionLevel.Assistant, RiskLevel.Low, false),
        [AiActionType.GenerateSupplierReport]  = new(AiActionCategory.Generate, AiPermissionLevel.Assistant, RiskLevel.Low, false),
        [AiActionType.GenerateSpcSummary]      = new(AiActionCategory.Generate, AiPermissionLevel.Assistant, RiskLevel.Low, false),
        [AiActionType.GeneratePowerPoint]      = new(AiActionCategory.Generate, AiPermissionLevel.Assistant, RiskLevel.Low, false),

        // ---- Level 2: AI Manager (DraftWorkflow) ----
        [AiActionType.CreateDraftWorkflow]       = new(AiActionCategory.DraftWorkflow, AiPermissionLevel.Manager, RiskLevel.Low, false),
        [AiActionType.AssignDraftTask]           = new(AiActionCategory.DraftWorkflow, AiPermissionLevel.Manager, RiskLevel.Low, false),
        [AiActionType.PrepareReport]             = new(AiActionCategory.DraftWorkflow, AiPermissionLevel.Manager, RiskLevel.Low, false),
        [AiActionType.GenerateManagementReview]  = new(AiActionCategory.DraftWorkflow, AiPermissionLevel.Manager, RiskLevel.Medium, false),
        [AiActionType.GenerateDashboard]         = new(AiActionCategory.DraftWorkflow, AiPermissionLevel.Manager, RiskLevel.Low, false),

        // ---- Level 3: AI Administrator (Write) ----
        [AiActionType.CreateRecord]                = new(AiActionCategory.Write, AiPermissionLevel.Administrator, RiskLevel.Medium, false),
        [AiActionType.UpdateRecord]                = new(AiActionCategory.Write, AiPermissionLevel.Administrator, RiskLevel.Medium, false),
        [AiActionType.ExecuteWorkflow]             = new(AiActionCategory.Write, AiPermissionLevel.Administrator, RiskLevel.Medium, true),
        [AiActionType.GenerateMonthlyReview]       = new(AiActionCategory.Write, AiPermissionLevel.Administrator, RiskLevel.Medium, false),
        [AiActionType.CreateScar]                  = new(AiActionCategory.Write, AiPermissionLevel.Administrator, RiskLevel.Medium, false),
        [AiActionType.CreateCapa]                  = new(AiActionCategory.Write, AiPermissionLevel.Administrator, RiskLevel.Medium, false),
        [AiActionType.GenerateAuditPlan]           = new(AiActionCategory.Write, AiPermissionLevel.Administrator, RiskLevel.Medium, false),
        [AiActionType.ScheduleCalibration]         = new(AiActionCategory.Write, AiPermissionLevel.Administrator, RiskLevel.Medium, false),
        [AiActionType.IssueTrainingAssignment]     = new(AiActionCategory.Write, AiPermissionLevel.Administrator, RiskLevel.Medium, false),
        [AiActionType.GenerateSupplierScorecard]   = new(AiActionCategory.Write, AiPermissionLevel.Administrator, RiskLevel.Medium, false),
        [AiActionType.BulkNotification]            = new(AiActionCategory.Write, AiPermissionLevel.Administrator, RiskLevel.High, true),
        [AiActionType.GenerateKpiDashboard]        = new(AiActionCategory.Write, AiPermissionLevel.Administrator, RiskLevel.Low, false),
        [AiActionType.ExecuteAutomation]           = new(AiActionCategory.Write, AiPermissionLevel.Administrator, RiskLevel.High, true),

        // ---- Level 4: AI Super Administrator (BulkOperation / CrossModuleOrchestration) ----
        [AiActionType.FullDatabaseAccess]                     = new(AiActionCategory.CrossModuleOrchestration, AiPermissionLevel.SuperAdministrator, RiskLevel.High, true),
        [AiActionType.BulkOperation]                          = new(AiActionCategory.BulkOperation, AiPermissionLevel.SuperAdministrator, RiskLevel.High, true),
        [AiActionType.CrossModuleOrchestration]               = new(AiActionCategory.CrossModuleOrchestration, AiPermissionLevel.SuperAdministrator, RiskLevel.High, true),
        [AiActionType.CrossModuleAnalysis]                    = new(AiActionCategory.CrossModuleOrchestration, AiPermissionLevel.SuperAdministrator, RiskLevel.Medium, false),
        [AiActionType.TrendAnalysis]                          = new(AiActionCategory.CrossModuleOrchestration, AiPermissionLevel.SuperAdministrator, RiskLevel.Low, false),
        [AiActionType.SupplierIntelligence]                   = new(AiActionCategory.CrossModuleOrchestration, AiPermissionLevel.SuperAdministrator, RiskLevel.Low, false),
        [AiActionType.SpcIntelligence]                        = new(AiActionCategory.CrossModuleOrchestration, AiPermissionLevel.SuperAdministrator, RiskLevel.Low, false),
        [AiActionType.PredictiveQualityAnalytics]             = new(AiActionCategory.CrossModuleOrchestration, AiPermissionLevel.SuperAdministrator, RiskLevel.Low, false),
        [AiActionType.AiPlanning]                             = new(AiActionCategory.CrossModuleOrchestration, AiPermissionLevel.SuperAdministrator, RiskLevel.Medium, false),
        [AiActionType.TaskScheduling]                         = new(AiActionCategory.CrossModuleOrchestration, AiPermissionLevel.SuperAdministrator, RiskLevel.Medium, true),
        [AiActionType.AutomaticReminderGeneration]            = new(AiActionCategory.CrossModuleOrchestration, AiPermissionLevel.SuperAdministrator, RiskLevel.Medium, false),
        [AiActionType.AutomaticMeetingPreparation]            = new(AiActionCategory.CrossModuleOrchestration, AiPermissionLevel.SuperAdministrator, RiskLevel.Medium, false),
        [AiActionType.AutomaticManagementReviewGeneration]    = new(AiActionCategory.CrossModuleOrchestration, AiPermissionLevel.SuperAdministrator, RiskLevel.Medium, true),
        [AiActionType.SystemDiagnostics]                      = new(AiActionCategory.CrossModuleOrchestration, AiPermissionLevel.SuperAdministrator, RiskLevel.Low, false),
        [AiActionType.ServerHealthMonitoring]                  = new(AiActionCategory.CrossModuleOrchestration, AiPermissionLevel.SuperAdministrator, RiskLevel.Low, false),
        [AiActionType.BackgroundJobManagement]                 = new(AiActionCategory.CrossModuleOrchestration, AiPermissionLevel.SuperAdministrator, RiskLevel.High, true),

        // ---- Dangerous (require confirmation at ANY level) ----
        [AiActionType.DeleteRecord]            = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.ArchiveRecord]           = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.DeactivateRecord]        = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.ApproveRecord]           = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.RejectRecord]            = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.CloseCapaRecord]         = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.CloseNcrRecord]          = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.CloseAuditRecord]        = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.CloseScarRecord]         = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.CloseImpactAssessment]   = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.BulkUpdate]              = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.BulkDelete]              = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.ConfigurationChange]     = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.UserManagement]          = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.PermissionChange]        = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.DatabaseMaintenance]     = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
        [AiActionType.AiModelChange]           = new(AiActionCategory.Dangerous, AiPermissionLevel.SuperAdministrator, RiskLevel.Critical, true),
    };

    /// <summary>
    /// Returns the <see cref="AiActionCategory"/> for the given action type.
    /// </summary>
    public static AiActionCategory GetCategory(AiActionType actionType) =>
        Rules.TryGetValue(actionType, out var rule) ? rule.Category : AiActionCategory.Dangerous;

    /// <summary>
    /// Returns the minimum <see cref="AiPermissionLevel"/> required to execute the action.
    /// </summary>
    public static AiPermissionLevel GetMinimumLevel(AiActionType actionType) =>
        Rules.TryGetValue(actionType, out var rule) ? rule.MinimumLevel : AiPermissionLevel.SuperAdministrator;

    /// <summary>
    /// Returns the <see cref="RiskLevel"/> for the given action type.
    /// </summary>
    public static RiskLevel GetRiskLevel(AiActionType actionType) =>
        Rules.TryGetValue(actionType, out var rule) ? rule.Risk : RiskLevel.Critical;

    /// <summary>
    /// Returns whether the action requires explicit user confirmation before execution.
    /// </summary>
    public static bool RequiresConfirmation(AiActionType actionType) =>
        Rules.TryGetValue(actionType, out var rule) ? rule.RequiresConfirmation : true;

    /// <summary>
    /// Checks whether the given permission level is sufficient for the action type.
    /// </summary>
    public static bool IsAuthorized(AiPermissionLevel userLevel, AiActionType actionType) =>
        (int)userLevel >= (int)GetMinimumLevel(actionType);
}
