using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.ProductCatalog.Entities;

public sealed class BomReference : Entity<Guid>
{
    public Guid ParentPartId { get; private set; }
    public Guid ChildPartId { get; private set; }
    public decimal Quantity { get; private set; }
    public string? ReferenceDesignator { get; private set; }
    public int SortOrder { get; private set; }

    private BomReference() { }

    internal static BomReference Create(
        Guid id,
        TenantId tenantId,
        Guid parentPartId,
        Guid childPartId,
        decimal quantity,
        string? referenceDesignator,
        int sortOrder)
    {
        if (parentPartId == childPartId)
            throw new DomainException("A part cannot reference itself in a BOM.");

        if (quantity <= 0)
            throw new DomainException("BOM quantity must be greater than zero.");

        return new BomReference
        {
            Id = id,
            TenantId = tenantId,
            ParentPartId = parentPartId,
            ChildPartId = childPartId,
            Quantity = quantity,
            ReferenceDesignator = referenceDesignator?.Trim(),
            SortOrder = sortOrder
        };
    }

    internal void Update(decimal quantity, string? referenceDesignator, int sortOrder)
    {
        if (quantity <= 0)
            throw new DomainException("BOM quantity must be greater than zero.");

        Quantity = quantity;
        ReferenceDesignator = referenceDesignator?.Trim();
        SortOrder = sortOrder;
    }
}
