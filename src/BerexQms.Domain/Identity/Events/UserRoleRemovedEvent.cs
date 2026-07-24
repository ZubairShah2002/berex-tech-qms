using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Identity.Events;

public sealed record UserRoleRemovedEvent(
    Guid UserId,
    Guid RoleId,
    string RoleName,
    Guid TenantId) : DomainEvent;
