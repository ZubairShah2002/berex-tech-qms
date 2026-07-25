using BerexQms.Domain.ProductCatalog.Entities;
using BerexQms.Domain.ProductCatalog.Enums;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.ProductCatalog.Configurations;

public sealed class PartRevisionConfiguration : IEntityTypeConfiguration<PartRevision>
{
    public void Configure(EntityTypeBuilder<PartRevision> builder)
    {
        builder.ToTable("part_revisions", "catalog");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(r => r.PartId)
            .HasColumnName("part_id")
            .IsRequired();

        builder.Property(r => r.RevisionCode)
            .HasColumnName("revision_code")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(r => r.ChangeReason)
            .HasColumnName("change_reason")
            .HasMaxLength(1000);

        builder.Property(r => r.ReleasedAt)
            .HasColumnName("released_at");

        builder.Property(r => r.ReleasedBy)
            .HasColumnName("released_by")
            .HasMaxLength(100);

        builder.Property(r => r.ObsoletedAt)
            .HasColumnName("obsoleted_at");

        builder.Property(r => r.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.ModifiedBy)
            .HasColumnName("modified_by")
            .HasMaxLength(100);

        builder.Property(r => r.ModifiedAt)
            .HasColumnName("modified_at");

        builder.HasMany(r => r.SpecificationParameters)
            .WithOne()
            .HasForeignKey(sp => sp.PartRevisionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.PartId, r.RevisionCode }).IsUnique()
            .HasDatabaseName("ix_part_revisions_part_code");
        builder.HasIndex(r => r.TenantId).HasDatabaseName("ix_part_revisions_tenant_id");
        builder.HasIndex(r => r.Status).HasDatabaseName("ix_part_revisions_status");

    }
}
