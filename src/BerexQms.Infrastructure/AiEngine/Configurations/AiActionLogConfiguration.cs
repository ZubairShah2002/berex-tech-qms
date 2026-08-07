using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AiEngine.Configurations;

public sealed class AiActionLogConfiguration : IEntityTypeConfiguration<AiActionLog>
{
    public void Configure(EntityTypeBuilder<AiActionLog> builder)
    {
        builder.ToTable("ai_action_logs", "ai_engine");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .HasConversion(v => v.Value, v => TenantId.From(v))
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.UserRole).HasColumnName("user_role").HasMaxLength(100).IsRequired();
        builder.Property(x => x.PermissionLevel).HasColumnName("permission_level").HasMaxLength(50).IsRequired();
        builder.Property(x => x.ActionType).HasColumnName("action_type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ActionCategory).HasColumnName("action_category").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Prompt).HasColumnName("prompt").HasMaxLength(10000);
        builder.Property(x => x.ReasoningSummary).HasColumnName("reasoning_summary").HasMaxLength(5000);
        builder.Property(x => x.AffectedModules).HasColumnName("affected_modules").HasMaxLength(500);
        builder.Property(x => x.AffectedRecords).HasColumnName("affected_records").HasMaxLength(5000);
        builder.Property(x => x.RiskLevel).HasColumnName("risk_level").HasMaxLength(20).IsRequired();
        builder.Property(x => x.ConfirmationStatus).HasColumnName("confirmation_status").HasMaxLength(20).IsRequired();
        builder.Property(x => x.RequiresConfirmation).HasColumnName("requires_confirmation").IsRequired();
        builder.Property(x => x.ConfirmedAt).HasColumnName("confirmed_at");
        builder.Property(x => x.ConfirmedBy).HasColumnName("confirmed_by").HasMaxLength(100);
        builder.Property(x => x.ExecutionResult).HasColumnName("execution_result").HasMaxLength(50).IsRequired();
        builder.Property(x => x.ErrorDetail).HasColumnName("error_detail").HasMaxLength(5000);
        builder.Property(x => x.RequestedAt).HasColumnName("requested_at").IsRequired();
        builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
        builder.Property(x => x.DurationMs).HasColumnName("duration_ms");
        builder.Property(x => x.ModelVersion).HasColumnName("model_version").HasMaxLength(100);
        builder.Property(x => x.ConfidenceScore).HasColumnName("confidence_score").HasPrecision(5, 4);
        builder.Property(x => x.IsRollbackPossible).HasColumnName("is_rollback_possible").IsRequired();

        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(x => x.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => new { x.TenantId, x.UserId })
            .HasDatabaseName("ix_ai_action_logs_tenant_user");

        builder.HasIndex(x => new { x.TenantId, x.ActionType })
            .HasDatabaseName("ix_ai_action_logs_tenant_action_type");

        builder.HasIndex(x => x.RequestedAt)
            .IsDescending()
            .HasDatabaseName("ix_ai_action_logs_requested_at");

        builder.HasIndex(x => new { x.TenantId, x.ConfirmationStatus })
            .HasDatabaseName("ix_ai_action_logs_tenant_confirmation");
    }
}
