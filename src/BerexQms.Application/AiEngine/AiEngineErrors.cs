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
}
