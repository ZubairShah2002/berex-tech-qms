using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Events;

public sealed record AiPermissionGrantedEvent(
    Guid UserId,
    string PermissionLevel,
    Guid GrantedByUserId) : DomainEvent;
