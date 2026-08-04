using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.SupplierQuality.Entities;

public sealed class SupplierApproval : Entity<Guid>
{
    public Guid SupplierId { get; private set; }
    public string ScopeDescription { get; private set; } = string.Empty;
    public DateTime ApprovedDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public string? Conditions { get; private set; }
    public bool IsActive { get; private set; }

    private SupplierApproval() { }

    internal static SupplierApproval Create(
        Guid id,
        TenantId tenantId,
        Guid supplierId,
        string scopeDescription,
        DateTime approvedDate,
        DateTime? expiryDate,
        string? conditions)
    {
        if (string.IsNullOrWhiteSpace(scopeDescription))
            throw new DomainException("Approval scope description is required.");

        return new SupplierApproval
        {
            Id = id,
            TenantId = tenantId,
            SupplierId = supplierId,
            ScopeDescription = scopeDescription.Trim(),
            ApprovedDate = approvedDate,
            ExpiryDate = expiryDate,
            Conditions = conditions?.Trim(),
            IsActive = true,
        };
    }

    internal void Revoke()
    {
        IsActive = false;
    }
}
