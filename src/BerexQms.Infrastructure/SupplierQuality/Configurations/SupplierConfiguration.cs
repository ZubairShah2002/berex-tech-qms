using BerexQms.Domain.SupplierQuality.Entities;
using BerexQms.Domain.SupplierQuality.ValueObjects;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.SupplierQuality.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers", "supplier");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(s => s.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(s => s.RiskLevel)
            .HasColumnName("risk_level")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Tier)
            .HasColumnName("tier")
            .HasMaxLength(50);

        builder.Property(s => s.ApprovedSince)
            .HasColumnName("approved_since");

        builder.OwnsOne(s => s.PrimaryContact, c =>
        {
            c.Property(x => x.Name)
                .HasColumnName("contact_name")
                .HasMaxLength(200);

            c.Property(x => x.Role)
                .HasColumnName("contact_role")
                .HasMaxLength(100);

            c.Property(x => x.Email)
                .HasColumnName("contact_email")
                .HasMaxLength(200);

            c.Property(x => x.Phone)
                .HasColumnName("contact_phone")
                .HasMaxLength(50);
        });

        builder.OwnsOne(s => s.RiskAssessment, ra =>
        {
            ra.Property(x => x.Level)
                .HasColumnName("risk_assessment_level")
                .HasConversion<string>()
                .HasMaxLength(20);

            ra.Property(x => x.ContributingFactors)
                .HasColumnName("risk_assessment_factors")
                .HasMaxLength(2000);

            ra.Property(x => x.AssessedAt)
                .HasColumnName("risk_assessed_at");
        });

        builder.Property(s => s.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.ModifiedBy)
            .HasColumnName("modified_by")
            .HasMaxLength(200);

        builder.Property(s => s.ModifiedAt)
            .HasColumnName("modified_at");

        builder.HasMany(s => s.Approvals)
            .WithOne()
            .HasForeignKey(a => a.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Scorecards)
            .WithOne()
            .HasForeignKey(sc => sc.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Scars)
            .WithOne()
            .HasForeignKey(sc => sc.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.ApprovedParts)
            .WithOne()
            .HasForeignKey(ap => ap.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.TenantId).HasDatabaseName("ix_suppliers_tenant_id");
        builder.HasIndex(s => new { s.TenantId, s.Code })
            .IsUnique()
            .HasDatabaseName("ix_suppliers_tenant_code");
        builder.HasIndex(s => new { s.TenantId, s.Status })
            .HasDatabaseName("ix_suppliers_tenant_status");
        builder.HasIndex(s => s.CreatedAt).HasDatabaseName("ix_suppliers_created_at");

        builder.Ignore(s => s.DomainEvents);
    }
}
