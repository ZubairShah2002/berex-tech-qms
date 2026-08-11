using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AiEngine.Configurations;

public sealed class AiGovernancePolicyConfiguration : IEntityTypeConfiguration<AiGovernancePolicy>
{
    public void Configure(EntityTypeBuilder<AiGovernancePolicy> builder)
    {
        builder.ToTable("ai_governance_policies", "ai_engine");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.IsAiEnabled).HasColumnName("is_ai_enabled").IsRequired();
        builder.Property(e => e.AllowedProvidersJson).HasColumnName("allowed_providers")
            .HasColumnType("jsonb");
        builder.Property(e => e.DefaultProvider).HasColumnName("default_provider").HasMaxLength(50);
        builder.Property(e => e.AllowedTaskTypesJson).HasColumnName("allowed_task_types")
            .HasColumnType("jsonb");
        builder.Property(e => e.MaxDailyRequestsPerUser).HasColumnName("max_daily_requests_per_user");
        builder.Property(e => e.MaxMonthlyRequestsPerTenant).HasColumnName("max_monthly_requests_per_tenant");
        builder.Property(e => e.RequireHumanConfirmation).HasColumnName("require_human_confirmation").IsRequired();

        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(e => e.DomainEvents);

        // One policy per tenant
        builder.HasIndex(e => e.TenantId)
            .IsUnique()
            .HasDatabaseName("ix_ai_governance_policies_tenant");
    }
}
