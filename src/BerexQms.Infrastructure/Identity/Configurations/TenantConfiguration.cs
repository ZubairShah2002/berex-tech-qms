using BerexQms.Domain.Identity.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Identity.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants", "identity");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(t => t.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(tid => tid.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(t => t.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Code)
            .HasColumnName("code")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(t => t.ContactEmail)
            .HasColumnName("contact_email")
            .HasMaxLength(254);

        builder.Property(t => t.TimeZone)
            .HasColumnName("timezone")
            .HasMaxLength(50)
            .HasDefaultValue("UTC");

        builder.Property(t => t.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.ModifiedBy)
            .HasColumnName("modified_by")
            .HasMaxLength(100);

        builder.Property(t => t.ModifiedAt)
            .HasColumnName("modified_at");

        builder.HasIndex(t => t.Code)
            .IsUnique()
            .HasDatabaseName("ix_tenants_code");

        builder.Ignore(t => t.DomainEvents);
    }
}
