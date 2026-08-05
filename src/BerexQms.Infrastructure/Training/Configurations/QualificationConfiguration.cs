using BerexQms.Domain.Training.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Training.Configurations;

public sealed class QualificationConfiguration : IEntityTypeConfiguration<Qualification>
{
    public void Configure(EntityTypeBuilder<Qualification> builder)
    {
        builder.ToTable("qualifications", "training");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(q => q.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(q => q.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(q => q.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(q => q.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(q => q.ScopeProductFamily)
            .HasColumnName("scope_product_family")
            .HasMaxLength(200);

        builder.Property(q => q.ScopeInspectionType)
            .HasColumnName("scope_inspection_type")
            .HasMaxLength(200);

        builder.Property(q => q.ScopeProcessArea)
            .HasColumnName("scope_process_area")
            .HasMaxLength(200);

        builder.Property(q => q.ValidityMonths)
            .HasColumnName("validity_months")
            .IsRequired();

        builder.Property(q => q.RenewalWindowDays)
            .HasColumnName("renewal_window_days")
            .IsRequired();

        builder.Property(q => q.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(q => q.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(q => q.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(q => q.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(q => q.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(q => q.DomainEvents);

        builder.HasIndex(q => new { q.TenantId, q.Code })
            .IsUnique()
            .HasDatabaseName("ix_qualifications_tenant_code");
        builder.HasIndex(q => new { q.TenantId, q.IsActive })
            .HasDatabaseName("ix_qualifications_tenant_active");
    }
}
