using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.ValueObjects;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AiEngine.Configurations;

public sealed class AiInteractionConfiguration : IEntityTypeConfiguration<AiInteraction>
{
    public void Configure(EntityTypeBuilder<AiInteraction> builder)
    {
        builder.ToTable("ai_interactions", "ai_engine");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.Capability).HasColumnName("capability").HasMaxLength(50).IsRequired();
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.ModelId).HasColumnName("model_id").HasMaxLength(200);
        builder.Property(e => e.InputSummary).HasColumnName("input_summary").HasMaxLength(2000);
        builder.Property(e => e.OutputSummary).HasColumnName("output_summary");
        builder.Property(e => e.SourceReferences).HasColumnName("source_references");
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(e => e.UserAction).HasColumnName("user_action").HasMaxLength(50);
        builder.Property(e => e.UserJustification).HasColumnName("user_justification").HasMaxLength(1000);
        builder.Property(e => e.RequestedAt).HasColumnName("requested_at").IsRequired();
        builder.Property(e => e.CompletedAt).HasColumnName("completed_at");
        builder.Property(e => e.ResponseTimeMs).HasColumnName("response_time_ms");

        // ConfidenceScore value object — flattened
        builder.OwnsOne(e => e.Confidence, cs =>
        {
            cs.Property(x => x.Score).HasColumnName("confidence_score").HasPrecision(5, 4);
            cs.Property(x => x.Level).HasColumnName("confidence_level").HasMaxLength(50)
                .HasConversion(l => l.ToString(), s => Enum.Parse<Domain.AiEngine.Enums.ConfidenceLevel>(s));
            cs.Ignore(x => x.IsSuppressed);
        });

        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(e => e.DomainEvents);

        builder.HasIndex(e => new { e.TenantId, e.Capability })
            .HasDatabaseName("ix_ai_interactions_tenant_capability");
        builder.HasIndex(e => new { e.TenantId, e.UserId })
            .HasDatabaseName("ix_ai_interactions_tenant_user");
        builder.HasIndex(e => new { e.TenantId, e.Status })
            .HasDatabaseName("ix_ai_interactions_tenant_status");
        builder.HasIndex(e => new { e.TenantId, e.RequestedAt })
            .HasDatabaseName("ix_ai_interactions_requested_at");
    }
}
