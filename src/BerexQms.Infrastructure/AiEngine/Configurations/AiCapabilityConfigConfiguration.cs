using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AiEngine.Configurations;

public sealed class AiCapabilityConfigConfiguration : IEntityTypeConfiguration<AiCapabilityConfig>
{
    public void Configure(EntityTypeBuilder<AiCapabilityConfig> builder)
    {
        builder.ToTable("ai_capability_configs", "ai_engine");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.Capability).HasColumnName("capability").HasMaxLength(50).IsRequired();
        builder.Property(e => e.IsEnabled).HasColumnName("is_enabled").IsRequired();
        builder.Property(e => e.LowConfidenceThreshold).HasColumnName("low_confidence_threshold").HasPrecision(5, 4).IsRequired();
        builder.Property(e => e.ModerateConfidenceThreshold).HasColumnName("moderate_confidence_threshold").HasPrecision(5, 4).IsRequired();
        builder.Property(e => e.HighConfidenceThreshold).HasColumnName("high_confidence_threshold").HasPrecision(5, 4).IsRequired();

        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(e => e.DomainEvents);

        builder.HasIndex(e => new { e.TenantId, e.Capability })
            .IsUnique()
            .HasDatabaseName("uq_ai_capability_configs_tenant_capability");
    }
}
