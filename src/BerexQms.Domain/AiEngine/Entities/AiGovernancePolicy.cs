using BerexQms.Domain.AiEngine.Events;
using BerexQms.SharedKernel;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AiEngine.Entities;

/// <summary>
/// Tenant-level AI governance policy controlling what AI features
/// are available, which providers are allowed, which tasks are permitted,
/// and usage limits.
///
/// One record per tenant (unique constraint on TenantId).
///
/// Governance policies always override user preferences:
/// a user cannot enable a provider or task that governance has restricted.
///
/// Default behavior (no explicit policy): AI enabled, all providers allowed,
/// all tasks allowed, no usage limits, human confirmation required.
/// </summary>
public sealed class AiGovernancePolicy : AggregateRoot<Guid>, IAuditableEntity
{
    /// <summary>Whether AI is enabled for the entire tenant.</summary>
    public bool IsAiEnabled { get; private set; }

    /// <summary>
    /// JSON-serialized list of allowed provider names (e.g. ["Local","Claude"]).
    /// Null means all registered providers are allowed.
    /// </summary>
    public string? AllowedProvidersJson { get; private set; }

    /// <summary>
    /// Tenant default provider. Null means use orchestrator routing.
    /// Must be one of the allowed providers.
    /// </summary>
    public string? DefaultProvider { get; private set; }

    /// <summary>
    /// JSON-serialized list of allowed task type names.
    /// Null means all task types are allowed.
    /// </summary>
    public string? AllowedTaskTypesJson { get; private set; }

    /// <summary>Max AI requests per user per day. Null means unlimited.</summary>
    public int? MaxDailyRequestsPerUser { get; private set; }

    /// <summary>Max AI requests per tenant per month. Null means unlimited.</summary>
    public int? MaxMonthlyRequestsPerTenant { get; private set; }

    /// <summary>
    /// Whether human confirmation is required for all AI-recommended QMS changes.
    /// Must remain true — cannot be set to false.
    /// </summary>
    public bool RequireHumanConfirmation { get; private set; }

    // IAuditableEntity
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private AiGovernancePolicy() { } // EF Core

    /// <summary>
    /// Create a new governance policy with permissive defaults.
    /// AI enabled, all providers/tasks allowed, no limits, confirmation required.
    /// </summary>
    public static AiGovernancePolicy Create(
        Guid id,
        TenantId tenantId)
    {
        var policy = new AiGovernancePolicy
        {
            Id = id,
            TenantId = tenantId,
            IsAiEnabled = true,
            AllowedProvidersJson = null, // All allowed
            DefaultProvider = null, // Orchestrator default
            AllowedTaskTypesJson = null, // All allowed
            MaxDailyRequestsPerUser = null, // Unlimited
            MaxMonthlyRequestsPerTenant = null, // Unlimited
            RequireHumanConfirmation = true,
        };

        policy.AddDomainEvent(new AiGovernancePolicyChangedEvent(
            id, tenantId.Value, "Created"));

        return policy;
    }

    /// <summary>Enable or disable AI for the entire tenant.</summary>
    public void SetAiEnabled(bool enabled)
    {
        if (IsAiEnabled == enabled) return;
        IsAiEnabled = enabled;

        AddDomainEvent(new AiGovernancePolicyChangedEvent(
            Id, TenantId.Value,
            enabled ? "AiEnabled" : "AiDisabled"));
    }

    /// <summary>
    /// Set allowed providers (JSON array string).
    /// Pass null to allow all providers.
    /// </summary>
    public void SetAllowedProviders(string? allowedProvidersJson)
    {
        AllowedProvidersJson = allowedProvidersJson;

        AddDomainEvent(new AiGovernancePolicyChangedEvent(
            Id, TenantId.Value, "AllowedProvidersChanged"));
    }

    /// <summary>Set default provider. Must be one of allowed providers.</summary>
    public void SetDefaultProvider(string? providerName)
    {
        DefaultProvider = string.IsNullOrWhiteSpace(providerName) ? null : providerName.Trim();
    }

    /// <summary>
    /// Set allowed task types (JSON array string).
    /// Pass null to allow all task types.
    /// </summary>
    public void SetAllowedTaskTypes(string? allowedTaskTypesJson)
    {
        AllowedTaskTypesJson = allowedTaskTypesJson;

        AddDomainEvent(new AiGovernancePolicyChangedEvent(
            Id, TenantId.Value, "AllowedTaskTypesChanged"));
    }

    /// <summary>Set per-user daily request limit. Null means unlimited.</summary>
    public void SetMaxDailyRequestsPerUser(int? maxRequests)
    {
        if (maxRequests is <= 0)
            throw new SharedKernel.Exceptions.DomainException(
                "Max daily requests per user must be positive or null.");

        MaxDailyRequestsPerUser = maxRequests;
    }

    /// <summary>Set tenant monthly request limit. Null means unlimited.</summary>
    public void SetMaxMonthlyRequestsPerTenant(int? maxRequests)
    {
        if (maxRequests is <= 0)
            throw new SharedKernel.Exceptions.DomainException(
                "Max monthly requests per tenant must be positive or null.");

        MaxMonthlyRequestsPerTenant = maxRequests;
    }

    /// <summary>
    /// Human confirmation is always required for QMS modifications.
    /// This method exists for completeness but cannot set the value to false.
    /// </summary>
    public void SetRequireHumanConfirmation(bool required)
    {
        // Human confirmation for QMS modifications is mandatory.
        // Even if someone passes false, we enforce true.
        RequireHumanConfirmation = true;
    }
}
