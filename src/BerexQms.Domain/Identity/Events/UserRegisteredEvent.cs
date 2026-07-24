using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Identity.Events;

public sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    Guid TenantId) : DomainEvent;
