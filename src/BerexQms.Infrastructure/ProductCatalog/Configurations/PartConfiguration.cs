using BerexQms.Domain.ProductCatalog.Entities;
using BerexQms.Domain.ProductCatalog.Enums;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.ProductCatalog.Configurations;

public sealed class PartConfiguration : IEntityTypeConfiguration<Part>
{
    public void Configure(EntityTypeBuilder<Part> builder)
    {
        builder.ToTable("parts", "catalog");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(p => p.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(p => p.PartNumber)
            .HasColumnName("part_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(p => p.ProductFamily)
            .HasColumnName("product_family")
            .HasMaxLength(100);

        builder.Property(p => p.Category)
            .HasColumnName("category")
            .HasMaxLength(100);

        builder.Property(p => p.SerializationMode)
            .HasColumnName("serialization_mode")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.UnitOfMeasure)
            .HasColumnName("unit_of_measure")
            .HasMaxLength(20);

        builder.Property(p => p.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.ModifiedBy)
            .HasColumnName("modified_by")
            .HasMaxLength(100);

        builder.Property(p => p.ModifiedAt)
            .HasColumnName("modified_at");

        builder.HasMany(p => p.Revisions)
            .WithOne()
            .HasForeignKey(r => r.PartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.BomReferences)
            .WithOne()
            .HasForeignKey(b => b.ParentPartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.TenantId).HasDatabaseName("ix_parts_tenant_id");
        builder.HasIndex(p => new { p.TenantId, p.PartNumber }).IsUnique().HasDatabaseName("ix_parts_tenant_part_number");
        builder.HasIndex(p => p.ProductFamily).HasDatabaseName("ix_parts_product_family");
        builder.HasIndex(p => p.Status).HasDatabaseName("ix_parts_status");

        builder.Ignore(p => p.DomainEvents);
    }
}
