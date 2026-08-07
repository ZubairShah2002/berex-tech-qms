using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AiEngine.Configurations;

public sealed class AiWorkflowExecutionConfiguration : IEntityTypeConfiguration<AiWorkflowExecution>
{
    public void Configure(EntityTypeBuilder<AiWorkflowExecution> builder)
    {
        builder.ToTable("ai_workflow_executions", "ai_engine");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .HasConversion(v => v.Value, v => TenantId.From(v))
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(x => x.WorkflowDefinitionId).HasColumnName("workflow_definition_id").IsRequired();
        builder.Property(x => x.WorkflowName).HasColumnName("workflow_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(x => x.TotalSteps).HasColumnName("total_steps").IsRequired();
        builder.Property(x => x.CompletedSteps).HasColumnName("completed_steps").IsRequired();
        builder.Property(x => x.FailedSteps).HasColumnName("failed_steps").IsRequired();
        builder.Property(x => x.StepResults).HasColumnName("step_results").HasColumnType("jsonb");
        builder.Property(x => x.Output).HasColumnName("output").HasColumnType("jsonb");
        builder.Property(x => x.StartedAt).HasColumnName("started_at").IsRequired();
        builder.Property(x => x.CompletedAt).HasColumnName("completed_at");
        builder.Property(x => x.TotalDurationMs).HasColumnName("total_duration_ms");
        builder.Property(x => x.ErrorSummary).HasColumnName("error_summary").HasMaxLength(5000);

        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(x => x.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => new { x.TenantId, x.UserId })
            .HasDatabaseName("ix_ai_workflow_executions_tenant_user");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_ai_workflow_executions_tenant_status");

        builder.HasIndex(x => x.StartedAt)
            .IsDescending()
            .HasDatabaseName("ix_ai_workflow_executions_started_at");
    }
}
