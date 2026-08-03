using BerexQms.Domain.DocumentControl.Entities;
using BerexQms.Domain.DocumentControl.Enums;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.DocumentControl.Configurations;

public sealed class DocumentMasterConfiguration : IEntityTypeConfiguration<DocumentMaster>
{
    public void Configure(EntityTypeBuilder<DocumentMaster> builder)
    {
        builder.ToTable("document_masters", "document");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(d => d.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(d => d.DocumentNumber)
            .HasColumnName("document_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(d => d.DocumentType)
            .HasColumnName("document_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(d => d.OwnerId)
            .HasColumnName("owner_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.Department)
            .HasColumnName("department")
            .HasMaxLength(100);

        builder.Property(d => d.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(d => d.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(d => d.ModifiedBy)
            .HasColumnName("modified_by")
            .HasMaxLength(200);

        builder.Property(d => d.ModifiedAt)
            .HasColumnName("modified_at");

        builder.HasMany(d => d.Versions)
            .WithOne()
            .HasForeignKey(v => v.DocumentMasterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.TenantId).HasDatabaseName("ix_document_masters_tenant_id");
        builder.HasIndex(d => new { d.TenantId, d.DocumentNumber })
            .IsUnique()
            .HasDatabaseName("ix_document_masters_tenant_number");
        builder.HasIndex(d => d.DocumentType).HasDatabaseName("ix_document_masters_type");
        builder.HasIndex(d => d.CreatedAt).HasDatabaseName("ix_document_masters_created_at");

        builder.Ignore(d => d.DomainEvents);
    }
}
