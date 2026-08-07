namespace BerexQms.Domain.AiEngine.Enums;

/// <summary>
/// Enumerates the specific AI actions that the system supports.
/// Each action maps to a <see cref="AiActionCategory"/> and a minimum
/// <see cref="AiPermissionLevel"/>.
/// </summary>
public enum AiActionType
{
    // ---- Level 1: AI Assistant (ReadOnly / Generate) ----

    ReadData,
    SearchDocuments,
    ExplainSop,
    ExplainProcedure,
    SummarizeReport,
    GenerateEmail,
    GenerateCapaDraft,
    GenerateNcrDraft,
    GenerateAuditDraft,
    GenerateSupplierReport,
    GenerateSpcSummary,
    GeneratePowerPoint,
    AnswerQuestion,

    // ---- Level 2: AI Manager (DraftWorkflow) ----

    CreateDraftWorkflow,
    AssignDraftTask,
    PrepareReport,
    GenerateManagementReview,
    GenerateDashboard,

    // ---- Level 3: AI Administrator (Write) ----

    CreateRecord,
    UpdateRecord,
    ExecuteWorkflow,
    GenerateMonthlyReview,
    CreateScar,
    CreateCapa,
    GenerateAuditPlan,
    ScheduleCalibration,
    IssueTrainingAssignment,
    GenerateSupplierScorecard,
    BulkNotification,
    GenerateKpiDashboard,
    ExecuteAutomation,

    // ---- Level 4: AI Super Administrator (BulkOperation / CrossModuleOrchestration) ----

    FullDatabaseAccess,
    BulkOperation,
    CrossModuleOrchestration,
    CrossModuleAnalysis,
    TrendAnalysis,
    SupplierIntelligence,
    SpcIntelligence,
    PredictiveQualityAnalytics,
    AiPlanning,
    TaskScheduling,
    AutomaticReminderGeneration,
    AutomaticMeetingPreparation,
    AutomaticManagementReviewGeneration,
    SystemDiagnostics,
    ServerHealthMonitoring,
    BackgroundJobManagement,

    // ---- Dangerous (require confirmation at any level) ----

    DeleteRecord,
    ArchiveRecord,
    DeactivateRecord,
    ApproveRecord,
    RejectRecord,
    CloseCapaRecord,
    CloseNcrRecord,
    CloseAuditRecord,
    CloseScarRecord,
    CloseImpactAssessment,
    BulkUpdate,
    BulkDelete,
    ConfigurationChange,
    UserManagement,
    PermissionChange,
    DatabaseMaintenance,
    AiModelChange,
}
