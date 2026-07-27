using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.NonConformance.Entities;

public sealed class ContainmentAction : Entity<Guid>
{
    public Guid NonConformanceId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string ActionTakenBy { get; private set; } = string.Empty;
    public DateTime ActionTakenAt { get; private set; }
    public bool IsVerified { get; private set; }
    public string? VerifiedBy { get; private set; }
    public DateTime? VerifiedAt { get; private set; }

    private ContainmentAction() { }

    internal static ContainmentAction Create(
        Guid id,
        TenantId tenantId,
        Guid nonConformanceId,
        string description,
        string actionTakenBy)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Containment action description is required.");

        if (string.IsNullOrWhiteSpace(actionTakenBy))
            throw new DomainException("Action taken by is required.");

        return new ContainmentAction
        {
            Id = id,
            TenantId = tenantId,
            NonConformanceId = nonConformanceId,
            Description = description.Trim(),
            ActionTakenBy = actionTakenBy,
            ActionTakenAt = DateTime.UtcNow,
            IsVerified = false
        };
    }

    internal void Verify(string verifiedBy)
    {
        if (string.IsNullOrWhiteSpace(verifiedBy))
            throw new DomainException("Verified by is required.");

        if (IsVerified)
            throw new DomainException("Containment action is already verified.");

        IsVerified = true;
        VerifiedBy = verifiedBy;
        VerifiedAt = DateTime.UtcNow;
    }
}
