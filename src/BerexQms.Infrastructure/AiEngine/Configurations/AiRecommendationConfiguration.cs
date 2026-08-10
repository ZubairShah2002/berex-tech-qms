using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AiEngine.Configurations;

public sealed class AiRecommendationConfiguration : IEntityTypeConfiguration<AiRecommendation>
{
    public void Configure(EntityTypeBuilder<AiRecommendation> builder)
    {
        builder.ToTable("ai_recommendations", "ai_engine");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.RecommendationType).HasColumnName("recommendation_type").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Title).HasColumnName("title").HasMaxLength(500).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(5000).IsRequired();
        builder.Property(e => e.Severity).HasColumnName("severity").HasMaxLength(20).IsRequired();
        builder.Property(e => e.SourceContextIds).HasColumnName("source_context_ids").HasMaxLength(2000);
        builder.Property(e => e.RelatedModule).HasColumnName("related_module").HasMaxLength(100).IsRequired();
        builder.Property(e => e.RelatedEntityId).HasColumnName("related_entity_id").HasMaxLength(200);
        builder.Property(e => e.ConfidenceScore).HasColumnName("confidence_score").HasPrecision(5, 4).IsRequired();
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(5000).IsRequired();
        builder.Property(e => e.SupportingData).HasColumnName("supporting_data");
        builder.Property(e => e.RecommendedAction).HasColumnName("recommended_action").HasMaxLength(2000);
        builder.Property(e => e.ReviewedAt).HasColumnName("reviewed_at");
        builder.Property(e => e.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(100);
        builder.Property(e => e.ReviewNotes).HasColumnName("review_notes").HasMaxLength(2000);

        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(e => e.DomainEvents);

        builder.HasIndex(e => new { e.TenantId, e.RecommendationType })
            .HasDatabaseName("ix_ai_recommendations_tenant_type");
        builder.HasIndex(e => new { e.TenantId, e.Status })
            .HasDatabaseName("ix_ai_recommendations_tenant_status");
        builder.HasIndex(e => new { e.TenantId, e.Severity })
            .HasDatabaseName("ix_ai_recommendations_tenant_severity");
        builder.HasIndex(e => new { e.TenantId, e.RelatedModule })
            .HasDatabaseName("ix_ai_recommendations_tenant_module");
        builder.HasIndex(e => new { e.TenantId, e.CreatedAt })
            .HasDatabaseName("ix_ai_recommendations_tenant_created");
    }
}
