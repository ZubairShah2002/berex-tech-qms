using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AiEngine.Configurations;

public sealed class AiUserPreferenceConfiguration : IEntityTypeConfiguration<AiUserPreference>
{
    public void Configure(EntityTypeBuilder<AiUserPreference> builder)
    {
        builder.ToTable("ai_user_preferences", "ai_engine");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.IsAiEnabled).HasColumnName("is_ai_enabled").IsRequired();
        builder.Property(e => e.PreferredProvider).HasColumnName("preferred_provider").HasMaxLength(50);
        builder.Property(e => e.RequireConfirmationForRecommendations)
            .HasColumnName("require_confirmation").IsRequired();
        builder.Property(e => e.PreferredLanguage).HasColumnName("preferred_language").HasMaxLength(10);
        builder.Property(e => e.EnabledTaskTypesJson).HasColumnName("enabled_task_types")
            .HasColumnType("jsonb");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(e => e.DomainEvents);

        // One preference per user per tenant
        builder.HasIndex(e => new { e.TenantId, e.UserId })
            .IsUnique()
            .HasDatabaseName("ix_ai_user_preferences_tenant_user");
    }
}
