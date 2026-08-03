using BerexQms.Domain.DocumentControl.Entities;
using BerexQms.Domain.DocumentControl.Enums;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.DocumentControl.Configurations;

public sealed class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("document_versions", "document");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(v => v.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(v => v.DocumentMasterId)
            .HasColumnName("document_master_id")
            .IsRequired();

        builder.Property(v => v.VersionNumber)
            .HasColumnName("version_number")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(v => v.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(v => v.Content)
            .HasColumnName("content")
            .IsRequired();

        builder.Property(v => v.ChangeDescription)
            .HasColumnName("change_description")
            .HasMaxLength(2000);

        builder.Property(v => v.AuthorId)
            .HasColumnName("author_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(v => v.EffectiveDate)
            .HasColumnName("effective_date");

        builder.Property(v => v.ExpiryDate)
            .HasColumnName("expiry_date");

        builder.OwnsOne(v => v.Attachment, a =>
        {
            a.Property(x => x.FileName)
                .HasColumnName("attachment_file_name")
                .HasMaxLength(255);

            a.Property(x => x.ContentType)
                .HasColumnName("attachment_content_type")
                .HasMaxLength(100);

            a.Property(x => x.SizeBytes)
                .HasColumnName("attachment_size_bytes");

            a.Property(x => x.StoragePath)
                .HasColumnName("attachment_storage_path")
                .HasMaxLength(500);

            a.Property(x => x.ContentHash)
                .HasColumnName("attachment_content_hash")
                .HasMaxLength(128);
        });

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(v => v.ReleasedAt)
            .HasColumnName("released_at");

        builder.Property(v => v.ReleasedBy)
            .HasColumnName("released_by")
            .HasMaxLength(200);

        builder.HasIndex(v => v.DocumentMasterId).HasDatabaseName("ix_document_versions_master_id");
        builder.HasIndex(v => v.TenantId).HasDatabaseName("ix_document_versions_tenant_id");
        builder.HasIndex(v => v.Status).HasDatabaseName("ix_document_versions_status");
    }
}
