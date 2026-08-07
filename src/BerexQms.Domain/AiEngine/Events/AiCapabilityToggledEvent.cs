using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Events;

/// <summary>
/// Raised when a tenant's kill switch for an AI capability is toggled on or off.
/// </summary>
public sealed record AiCapabilityToggledEvent(
    string Capability,
    bool IsEnabled,
    Guid ToggledByUserId) : DomainEvent;
