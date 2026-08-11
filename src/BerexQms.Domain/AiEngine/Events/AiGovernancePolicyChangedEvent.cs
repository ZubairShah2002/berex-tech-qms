using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Events;

/// <summary>
/// Raised when a tenant's AI governance policy is created or modified.
/// </summary>
public sealed record AiGovernancePolicyChangedEvent(
    Guid PolicyId,
    Guid TenantId,
    string ChangeDescription) : DomainEvent;
