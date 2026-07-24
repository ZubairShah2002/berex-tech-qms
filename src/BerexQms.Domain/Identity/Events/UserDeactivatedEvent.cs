using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Identity.Events;

public sealed record UserDeactivatedEvent(
    Guid UserId,
    Guid TenantId) : DomainEvent;
