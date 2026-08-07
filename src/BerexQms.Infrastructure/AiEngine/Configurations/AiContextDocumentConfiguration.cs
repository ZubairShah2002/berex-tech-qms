using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AiEngine.Configurations;

public sealed class AiContextDocumentConfiguration : IEntityTypeConfiguration<AiContextDocument>
{
    public void Configure(EntityTypeBuilder<AiContextDocument> builder)
    {
        builder.ToTable("ai_context_documents", "ai_engine");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.SourceModule).HasColumnName("source_module").HasMaxLength(100).IsRequired();
        builder.Property(e => e.SourceEntityId).HasColumnName("source_entity_id").HasMaxLength(200);
        builder.Property(e => e.ContextType).HasColumnName("context_type").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
        builder.Property(e => e.Content).HasColumnName("content").IsRequired();
        builder.Property(e => e.MetadataJson).HasColumnName("metadata_json");
        builder.Property(e => e.EmbeddingStatus).HasColumnName("embedding_status").HasMaxLength(50).IsRequired();
        builder.Property(e => e.IndexedAt).HasColumnName("indexed_at");
        builder.Property(e => e.IndexError).HasColumnName("index_error").HasMaxLength(2000);
        builder.Property(e => e.ContentVersion).HasColumnName("content_version").IsRequired();

        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(e => e.DomainEvents);

        builder.HasIndex(e => new { e.TenantId, e.SourceModule })
            .HasDatabaseName("ix_ai_context_documents_tenant_module");
        builder.HasIndex(e => new { e.TenantId, e.ContextType })
            .HasDatabaseName("ix_ai_context_documents_tenant_context_type");
        builder.HasIndex(e => new { e.TenantId, e.EmbeddingStatus })
            .HasDatabaseName("ix_ai_context_documents_tenant_status");
        builder.HasIndex(e => new { e.TenantId, e.SourceModule, e.SourceEntityId })
            .HasDatabaseName("ix_ai_context_documents_tenant_source_entity")
            .IsUnique()
            .HasFilter("source_entity_id IS NOT NULL");
    }
}
