using BerexQms.Domain.SupplierQuality.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.SupplierQuality.Configurations;

public sealed class SupplierApprovalConfiguration : IEntityTypeConfiguration<SupplierApproval>
{
    public void Configure(EntityTypeBuilder<SupplierApproval> builder)
    {
        builder.ToTable("supplier_approvals", "supplier");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(a => a.SupplierId)
            .HasColumnName("supplier_id")
            .IsRequired();

        builder.Property(a => a.ScopeDescription)
            .HasColumnName("scope_description")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(a => a.ApprovedDate)
            .HasColumnName("approved_date")
            .IsRequired();

        builder.Property(a => a.ExpiryDate)
            .HasColumnName("expiry_date");

        builder.Property(a => a.Conditions)
            .HasColumnName("conditions")
            .HasMaxLength(2000);

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasIndex(a => new { a.SupplierId, a.ExpiryDate })
            .HasDatabaseName("ix_supplier_approvals_supplier_expiry");
        builder.HasIndex(a => a.TenantId)
            .HasDatabaseName("ix_supplier_approvals_tenant_id");
    }
}
