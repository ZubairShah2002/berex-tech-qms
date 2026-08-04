using BerexQms.Domain.SupplierQuality.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.SupplierQuality.Configurations;

public sealed class ApprovedPartConfiguration : IEntityTypeConfiguration<ApprovedPart>
{
    public void Configure(EntityTypeBuilder<ApprovedPart> builder)
    {
        builder.ToTable("approved_parts", "supplier");

        builder.HasKey(ap => ap.Id);
        builder.Property(ap => ap.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(ap => ap.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(ap => ap.SupplierId)
            .HasColumnName("supplier_id")
            .IsRequired();

        builder.Property(ap => ap.PartId)
            .HasColumnName("part_id")
            .IsRequired();

        builder.Property(ap => ap.RevisionScope)
            .HasColumnName("revision_scope")
            .HasMaxLength(200);

        builder.Property(ap => ap.ApprovalDate)
            .HasColumnName("approval_date")
            .IsRequired();

        builder.Property(ap => ap.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasIndex(ap => new { ap.SupplierId, ap.PartId })
            .IsUnique()
            .HasFilter("is_active = true")
            .HasDatabaseName("ix_approved_parts_supplier_part");
        builder.HasIndex(ap => ap.TenantId)
            .HasDatabaseName("ix_approved_parts_tenant_id");
    }
}
