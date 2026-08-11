using BerexQms.Domain.AiEngine.Events;
using BerexQms.SharedKernel;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AiEngine.Entities;

/// <summary>
/// Per-user, per-tenant AI preference settings.
/// Controls whether a user wants AI assistance, their preferred provider,
/// and which AI tasks they want enabled.
///
/// User preferences are subordinate to governance policies:
/// a preference can never override a governance restriction.
///
/// One record per user per tenant (unique constraint).
/// </summary>
public sealed class AiUserPreference : AggregateRoot<Guid>, IAuditableEntity
{
    /// <summary>User this preference belongs to.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Whether the user wants AI assistance enabled.</summary>
    public bool IsAiEnabled { get; private set; }

    /// <summary>
    /// User's preferred provider name (e.g. "Local", "Claude", "OpenAi").
    /// Null or empty means "Automatic" — use the orchestrator's default routing.
    /// This is a preference, not authority; governance may override it.
    /// </summary>
    public string? PreferredProvider { get; private set; }

    /// <summary>
    /// Whether the user always wants a confirmation prompt before
    /// AI recommendations are applied. Defaults to true.
    /// Cannot be set to false if governance mandates confirmation.
    /// </summary>
    public bool RequireConfirmationForRecommendations { get; private set; }

    /// <summary>
    /// User's preferred language for AI responses (ISO 639-1 code).
    /// Null means system default.
    /// </summary>
    public string? PreferredLanguage { get; private set; }

    /// <summary>
    /// JSON-serialized list of AI task types the user has enabled.
    /// Null means "all governance-allowed tasks enabled" (opt-out model).
    /// When set, only listed tasks that are also governance-allowed are active.
    /// </summary>
    public string? EnabledTaskTypesJson { get; private set; }

    // IAuditableEntity
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private AiUserPreference() { } // EF Core

    /// <summary>
    /// Create a new user preference with defaults.
    /// AI enabled, automatic provider, confirmation required.
    /// </summary>
    public static AiUserPreference Create(
        Guid id,
        TenantId tenantId,
        Guid userId)
    {
        var preference = new AiUserPreference
        {
            Id = id,
            TenantId = tenantId,
            UserId = userId,
            IsAiEnabled = true,
            PreferredProvider = null, // Automatic
            RequireConfirmationForRecommendations = true,
            PreferredLanguage = null,
            EnabledTaskTypesJson = null, // All allowed
        };

        preference.AddDomainEvent(new AiUserPreferenceChangedEvent(
            id, userId, tenantId.Value, "Created"));

        return preference;
    }

    /// <summary>Enable or disable AI assistance for this user.</summary>
    public void SetAiEnabled(bool enabled)
    {
        if (IsAiEnabled == enabled) return;

        IsAiEnabled = enabled;
        AddDomainEvent(new AiUserPreferenceChangedEvent(
            Id, UserId, TenantId.Value,
            enabled ? "AiEnabled" : "AiDisabled"));
    }

    /// <summary>
    /// Set the preferred provider. Pass null for Automatic.
    /// Validation against governance policy occurs in the application layer.
    /// </summary>
    public void SetPreferredProvider(string? providerName)
    {
        var normalized = string.IsNullOrWhiteSpace(providerName) ? null : providerName.Trim();

        if (PreferredProvider == normalized) return;

        var oldProvider = PreferredProvider ?? "Automatic";
        PreferredProvider = normalized;

        AddDomainEvent(new AiUserPreferenceChangedEvent(
            Id, UserId, TenantId.Value,
            $"PreferredProvider:{oldProvider}->{normalized ?? "Automatic"}"));
    }

    /// <summary>Set confirmation preference.</summary>
    public void SetConfirmationPreference(bool requireConfirmation)
    {
        RequireConfirmationForRecommendations = requireConfirmation;
    }

    /// <summary>Set preferred language.</summary>
    public void SetPreferredLanguage(string? languageCode)
    {
        PreferredLanguage = string.IsNullOrWhiteSpace(languageCode) ? null : languageCode.Trim();
    }

    /// <summary>
    /// Set enabled task types (JSON array string).
    /// Pass null to enable all governance-allowed tasks.
    /// </summary>
    public void SetEnabledTaskTypes(string? enabledTaskTypesJson)
    {
        EnabledTaskTypesJson = enabledTaskTypesJson;
    }
}
