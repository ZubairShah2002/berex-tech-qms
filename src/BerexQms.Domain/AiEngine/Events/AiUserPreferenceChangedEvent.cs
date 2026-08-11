using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Events;

/// <summary>
/// Raised when a user's AI preference is created or modified.
/// </summary>
public sealed record AiUserPreferenceChangedEvent(
    Guid PreferenceId,
    Guid UserId,
    Guid TenantId,
    string ChangeDescription) : DomainEvent;
