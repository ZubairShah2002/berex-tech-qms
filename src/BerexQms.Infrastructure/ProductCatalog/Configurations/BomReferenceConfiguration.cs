using BerexQms.Domain.ProductCatalog.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.ProductCatalog.Configurations;

public sealed class BomReferenceConfiguration : IEntityTypeConfiguration<BomReference>
{
    public void Configure(EntityTypeBuilder<BomReference> builder)
    {
        builder.ToTable("bom_references", "catalog");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(b => b.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(b => b.ParentPartId)
            .HasColumnName("parent_part_id")
            .IsRequired();

        builder.Property(b => b.ChildPartId)
            .HasColumnName("child_part_id")
            .IsRequired();

        builder.Property(b => b.Quantity)
            .HasColumnName("quantity")
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(b => b.ReferenceDesignator)
            .HasColumnName("reference_designator")
            .HasMaxLength(100);

        builder.Property(b => b.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.HasIndex(b => b.TenantId).HasDatabaseName("ix_bom_refs_tenant_id");
        builder.HasIndex(b => new { b.ParentPartId, b.ChildPartId }).IsUnique()
            .HasDatabaseName("ix_bom_refs_parent_child");
        builder.HasIndex(b => b.ChildPartId).HasDatabaseName("ix_bom_refs_child_part_id");
    }
}
