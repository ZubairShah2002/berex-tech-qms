namespace BerexQms.Domain.AiEngine.Enums;

/// <summary>
/// Tracks the lifecycle of an AI-orchestrated workflow execution.
/// </summary>
public enum AiWorkflowStatus
{
    /// <summary>Workflow execution has been requested and is awaiting confirmation.</summary>
    PendingConfirmation,

    /// <summary>Workflow has been confirmed by the user and is executing steps.</summary>
    Running,

    /// <summary>Workflow completed all steps successfully.</summary>
    Completed,

    /// <summary>Workflow execution failed on one or more steps.</summary>
    Failed,

    /// <summary>Workflow was cancelled by the user before completion.</summary>
    Cancelled,
}
