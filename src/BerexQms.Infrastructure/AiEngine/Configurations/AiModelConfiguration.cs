using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AiEngine.Configurations;

public sealed class AiModelConfiguration : IEntityTypeConfiguration<AiModel>
{
    public void Configure(EntityTypeBuilder<AiModel> builder)
    {
        builder.ToTable("ai_models", "ai_engine");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Version).HasColumnName("version").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Capability).HasColumnName("capability").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(e => e.TrainingMetrics).HasColumnName("training_metrics");
        builder.Property(e => e.ValidationMetrics).HasColumnName("validation_metrics");
        builder.Property(e => e.HyperParameters).HasColumnName("hyper_parameters");
        builder.Property(e => e.DataSnapshotReference).HasColumnName("data_snapshot_reference").HasMaxLength(500);
        builder.Property(e => e.TrainingSampleCount).HasColumnName("training_sample_count");
        builder.Property(e => e.TrainedAt).HasColumnName("trained_at");
        builder.Property(e => e.PromotedAt).HasColumnName("promoted_at");
        builder.Property(e => e.RetiredAt).HasColumnName("retired_at");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(e => e.DomainEvents);

        builder.HasIndex(e => new { e.TenantId, e.Name, e.Version })
            .IsUnique()
            .HasDatabaseName("uq_ai_models_tenant_name_version");
        builder.HasIndex(e => new { e.TenantId, e.Capability })
            .HasDatabaseName("ix_ai_models_tenant_capability");
        builder.HasIndex(e => new { e.TenantId, e.Status })
            .HasDatabaseName("ix_ai_models_tenant_status");
    }
}
