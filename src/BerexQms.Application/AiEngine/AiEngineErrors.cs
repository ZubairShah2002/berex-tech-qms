using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine;

public static class AiEngineErrors
{
    public static readonly Error InteractionNotFound = Error.NotFound(
        "AiEngine.InteractionNotFound", "AI interaction not found.");

    public static readonly Error ModelNotFound = Error.NotFound(
        "AiEngine.ModelNotFound", "AI model not found.");

    public static readonly Error CapabilityConfigNotFound = Error.NotFound(
        "AiEngine.CapabilityConfigNotFound", "AI capability configuration not found.");

    public static readonly Error CapabilityDisabled = Error.Validation(
        "AiEngine.CapabilityDisabled", "The requested AI capability is currently disabled.");

    public static readonly Error ModelVersionExists = Error.Conflict(
        "AiEngine.ModelVersionExists", "A model with this name and version already exists.");

    public static readonly Error InvalidModelTransition = Error.Validation(
        "AiEngine.InvalidModelTransition", "The requested model status transition is not valid.");

    public static readonly Error JustificationRequired = Error.Validation(
        "AiEngine.JustificationRequired",
        "A justification is required when accepting a moderate-confidence AI suggestion.");

    public static readonly Error InvalidConfidenceThreshold = Error.Validation(
        "AiEngine.InvalidConfidenceThreshold",
        "Confidence thresholds must be between 0 and 1, with low < moderate < high, and low >= 0.20.");

    public static readonly Error InvalidCapability = Error.Validation(
        "AiEngine.InvalidCapability", "The specified AI capability is not recognized.");

    public static readonly Error InvalidUserAction = Error.Validation(
        "AiEngine.InvalidUserAction", "The specified user action is not recognized.");

    public static readonly Error InvalidModelStatus = Error.Validation(
        "AiEngine.InvalidModelStatus", "The specified model status is not recognized.");

    public static readonly Error InteractionNotCompleted = Error.Validation(
        "AiEngine.InteractionNotCompleted",
        "A user action can only be recorded on a completed AI interaction.");

    // ---- v2.0: AI Permission & Workflow Errors ----

    public static readonly Error InvalidPermissionLevel = Error.Validation(
        "AiEngine.InvalidPermissionLevel", "The specified AI permission level is not recognized.");

    public static readonly Error InsufficientAiPermission = Error.Forbidden(
        "AiEngine.InsufficientAiPermission",
        "Your AI permission level is insufficient for this action.");

    public static readonly Error InvalidActionType = Error.Validation(
        "AiEngine.InvalidActionType", "The specified AI action type is not recognized.");

    public static readonly Error ActionLogNotFound = Error.NotFound(
        "AiEngine.ActionLogNotFound", "AI action log entry not found.");

    public static readonly Error PermissionPolicyNotFound = Error.NotFound(
        "AiEngine.PermissionPolicyNotFound",
        "No active AI permission policy found for this user.");

    public static readonly Error WorkflowDefinitionNotFound = Error.NotFound(
        "AiEngine.WorkflowDefinitionNotFound", "AI workflow definition not found.");

    public static readonly Error WorkflowDefinitionInactive = Error.Validation(
        "AiEngine.WorkflowDefinitionInactive",
        "This workflow definition is currently inactive.");

    public static readonly Error WorkflowExecutionNotFound = Error.NotFound(
        "AiEngine.WorkflowExecutionNotFound", "AI workflow execution not found.");

    // ---- Sprint 14: AI Context Engine Errors ----

    public static readonly Error ContextDocumentNotFound = Error.NotFound(
        "AiEngine.ContextDocumentNotFound", "AI context document not found.");

    public static readonly Error InvalidContextType = Error.Validation(
        "AiEngine.InvalidContextType", "The specified context type is not recognized.");

    public static readonly Error KnowledgeSourceNotFound = Error.NotFound(
        "AiEngine.KnowledgeSourceNotFound", "AI knowledge source not found.");

    public static readonly Error KnowledgeSourceModuleExists = Error.Conflict(
        "AiEngine.KnowledgeSourceModuleExists",
        "A knowledge source for this module already exists.");

    public static readonly Error ContextDocumentAlreadyExists = Error.Conflict(
        "AiEngine.ContextDocumentAlreadyExists",
        "A context document for this source entity already exists.");

    // ---- Sprint 15: AI Recommendation & Quality Intelligence Errors ----

    public static readonly Error RecommendationNotFound = Error.NotFound(
        "AiEngine.RecommendationNotFound", "AI recommendation not found.");

    public static readonly Error InvalidRecommendationType = Error.Validation(
        "AiEngine.InvalidRecommendationType",
        "The specified recommendation type is not recognized.");

    public static readonly Error InvalidSeverity = Error.Validation(
        "AiEngine.InvalidSeverity", "The specified severity level is not recognized.");

    public static readonly Error InvalidReviewAction = Error.Validation(
        "AiEngine.InvalidReviewAction",
        "Invalid review action. Valid values: accept, reject, review.");
}
