using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine;

/// <summary>
/// Structured error definitions for AI governance violations.
/// Uses the Result pattern — no sensitive policy details are leaked.
/// </summary>
public static class AiGovernanceErrors
{
    public static readonly Error AiDisabled = Error.Forbidden(
        "AiGovernance.AiDisabled",
        "AI assistance is disabled for your account.");

    public static readonly Error AiDisabledByPolicy = Error.Forbidden(
        "AiGovernance.AiDisabledByPolicy",
        "AI assistance is disabled by your organization's policy.");

    public static Error TaskNotPermitted(string taskType) => Error.Forbidden(
        "AiGovernance.TaskNotPermitted",
        $"You do not have permission to use AI for '{taskType}'.");

    public static Error ProviderNotPermitted(string provider) => Error.Forbidden(
        "AiGovernance.ProviderNotPermitted",
        $"The AI provider '{provider}' is not available under the current policy.");

    public static readonly Error DailyLimitReached = Error.Forbidden(
        "AiGovernance.DailyLimitReached",
        "You have reached your daily AI request limit.");

    public static readonly Error MonthlyLimitReached = Error.Forbidden(
        "AiGovernance.MonthlyLimitReached",
        "Your organization has reached its monthly AI request limit.");

    public static readonly Error ConfirmationRequired = Error.Validation(
        "AiGovernance.ConfirmationRequired",
        "Human confirmation is required before this AI action can proceed.");

    public static readonly Error GovernanceAccessDenied = Error.Forbidden(
        "AiGovernance.AccessDenied",
        "You do not have permission to manage AI governance settings.");

    public static readonly Error InvalidProvider = Error.Validation(
        "AiGovernance.InvalidProvider",
        "The specified provider is not a recognized AI provider.");

    public static readonly Error InvalidTaskType = Error.Validation(
        "AiGovernance.InvalidTaskType",
        "The specified task type is not a recognized AI task type.");

    public static Error PreferenceNotFound(Guid userId) => Error.NotFound(
        "AiGovernance.PreferenceNotFound",
        $"No AI preference found for user '{userId}'.");
}
