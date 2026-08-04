using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.SupplierQuality.Entities;

public sealed class ApprovedPart : Entity<Guid>
{
    public Guid SupplierId { get; private set; }
    public Guid PartId { get; private set; }
    public string? RevisionScope { get; private set; }
    public DateTime ApprovalDate { get; private set; }
    public bool IsActive { get; private set; }

    private ApprovedPart() { }

    internal static ApprovedPart Create(
        Guid id,
        TenantId tenantId,
        Guid supplierId,
        Guid partId,
        string? revisionScope,
        DateTime approvalDate)
    {
        return new ApprovedPart
        {
            Id = id,
            TenantId = tenantId,
            SupplierId = supplierId,
            PartId = partId,
            RevisionScope = revisionScope?.Trim(),
            ApprovalDate = approvalDate,
            IsActive = true,
        };
    }

    internal void Deactivate()
    {
        IsActive = false;
    }
}
