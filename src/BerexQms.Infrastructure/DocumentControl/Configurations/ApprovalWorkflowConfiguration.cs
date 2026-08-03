using BerexQms.Domain.DocumentControl.Entities;
using BerexQms.Domain.DocumentControl.Enums;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.DocumentControl.Configurations;

public sealed class ApprovalWorkflowConfiguration : IEntityTypeConfiguration<ApprovalWorkflow>
{
    public void Configure(EntityTypeBuilder<ApprovalWorkflow> builder)
    {
        builder.ToTable("approval_workflows", "document");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(w => w.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(w => w.DocumentVersionId)
            .HasColumnName("document_version_id")
            .IsRequired();

        builder.Property(w => w.CurrentStepOrder)
            .HasColumnName("current_step_order")
            .IsRequired();

        builder.Property(w => w.IsComplete)
            .HasColumnName("is_complete")
            .IsRequired();

        builder.Property(w => w.IsRejected)
            .HasColumnName("is_rejected")
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(w => w.CompletedAt)
            .HasColumnName("completed_at");

        builder.OwnsMany(w => w.Steps, s =>
        {
            s.ToTable("approval_steps", "document");

            s.WithOwner().HasForeignKey("ApprovalWorkflowId");
            s.Property<int>("Id").ValueGeneratedOnAdd();
            s.HasKey("Id");

            s.Property(x => x.StepOrder)
                .HasColumnName("step_order")
                .IsRequired();

            s.Property(x => x.ApproverId)
                .HasColumnName("approver_id")
                .HasMaxLength(200)
                .IsRequired();

            s.Property(x => x.Decision)
                .HasColumnName("decision")
                .HasConversion<string?>()
                .HasMaxLength(30);

            s.Property(x => x.Comments)
                .HasColumnName("comments")
                .HasMaxLength(2000);

            s.Property(x => x.Signature)
                .HasColumnName("signature")
                .HasMaxLength(500);

            s.Property(x => x.DecidedAt)
                .HasColumnName("decided_at");

            s.Property<Guid>("ApprovalWorkflowId")
                .HasColumnName("approval_workflow_id");

            s.HasIndex("ApprovalWorkflowId").HasDatabaseName("ix_approval_steps_workflow_id");
        });

        builder.HasIndex(w => w.DocumentVersionId).HasDatabaseName("ix_approval_workflows_version_id");
        builder.HasIndex(w => w.TenantId).HasDatabaseName("ix_approval_workflows_tenant_id");
    }
}
