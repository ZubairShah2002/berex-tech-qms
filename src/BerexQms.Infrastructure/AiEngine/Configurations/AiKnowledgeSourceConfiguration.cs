using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AiEngine.Configurations;

public sealed class AiKnowledgeSourceConfiguration : IEntityTypeConfiguration<AiKnowledgeSource>
{
    public void Configure(EntityTypeBuilder<AiKnowledgeSource> builder)
    {
        builder.ToTable("ai_knowledge_sources", "ai_engine");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Module).HasColumnName("module").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(e => e.LastSyncedAt).HasColumnName("last_synced_at");
        builder.Property(e => e.DocumentCount).HasColumnName("document_count").IsRequired();

        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(e => e.DomainEvents);

        builder.HasIndex(e => new { e.TenantId, e.Module })
            .HasDatabaseName("ix_ai_knowledge_sources_tenant_module")
            .IsUnique();
        builder.HasIndex(e => new { e.TenantId, e.IsActive })
            .HasDatabaseName("ix_ai_knowledge_sources_tenant_active");
    }
}
