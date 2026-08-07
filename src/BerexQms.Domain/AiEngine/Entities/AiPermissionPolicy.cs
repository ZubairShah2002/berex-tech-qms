using BerexQms.Domain.AiEngine.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AiEngine.Entities;

/// <summary>
/// Maps a user (or role) to an <see cref="AiPermissionLevel"/> within a tenant.
/// When no policy exists for a user, the system defaults to Level 1 (Assistant).
/// Only one active policy per user per tenant is allowed.
/// </summary>
public sealed class AiPermissionPolicy : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid UserId { get; private set; }
    public string PermissionLevel { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public string? GrantedByUserId { get; private set; }
    public DateTime GrantedAt { get; private set; }
    public string? RevokedByUserId { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? Notes { get; private set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private AiPermissionPolicy() { }

    public static AiPermissionPolicy Create(
        Guid id,
        TenantId tenantId,
        Guid userId,
        AiPermissionLevel level,
        Guid grantedByUserId,
        string? notes)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User ID is required.");
        if (grantedByUserId == Guid.Empty)
            throw new DomainException("Granting user ID is required.");

        return new AiPermissionPolicy
        {
            Id = id,
            TenantId = tenantId,
            UserId = userId,
            PermissionLevel = level.ToString(),
            IsActive = true,
            GrantedByUserId = grantedByUserId.ToString(),
            GrantedAt = DateTime.UtcNow,
            Notes = notes,
        };
    }

    public void UpdateLevel(AiPermissionLevel newLevel, Guid updatedByUserId)
    {
        if (!IsActive)
            throw new DomainException("Cannot update level on a revoked permission policy.");

        PermissionLevel = newLevel.ToString();
        GrantedByUserId = updatedByUserId.ToString();
        GrantedAt = DateTime.UtcNow;
    }

    public void Revoke(Guid revokedByUserId)
    {
        if (!IsActive)
            throw new DomainException("Permission policy is already revoked.");

        IsActive = false;
        RevokedByUserId = revokedByUserId.ToString();
        RevokedAt = DateTime.UtcNow;
    }

    public void Reinstate(AiPermissionLevel level, Guid reinstatedByUserId)
    {
        if (IsActive)
            throw new DomainException("Permission policy is already active.");

        IsActive = true;
        PermissionLevel = level.ToString();
        GrantedByUserId = reinstatedByUserId.ToString();
        GrantedAt = DateTime.UtcNow;
        RevokedByUserId = null;
        RevokedAt = null;
    }
}
