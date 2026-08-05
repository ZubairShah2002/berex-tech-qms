using BerexQms.Domain.Training.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Training.Configurations;

public sealed class TrainingAssignmentConfiguration : IEntityTypeConfiguration<TrainingAssignment>
{
    public void Configure(EntityTypeBuilder<TrainingAssignment> builder)
    {
        builder.ToTable("training_assignments", "training");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(a => a.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();

        builder.Property(a => a.CourseId)
            .HasColumnName("course_id")
            .IsRequired();

        builder.Property(a => a.AssignedBy)
            .HasColumnName("assigned_by")
            .IsRequired();

        builder.Property(a => a.AssignedDate)
            .HasColumnName("assigned_date")
            .IsRequired();

        builder.Property(a => a.DueDate)
            .HasColumnName("due_date")
            .IsRequired();

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.OwnsOne(a => a.Completion, c =>
        {
            c.Property(x => x.CompletionDate).HasColumnName("completion_date");
            c.Property(x => x.Score).HasColumnName("score").HasPrecision(6, 2);
            c.Property(x => x.Result).HasColumnName("result").HasMaxLength(50);
            c.Property(x => x.AssessorId).HasColumnName("assessor_id");
            c.Property(x => x.EvidenceRef).HasColumnName("evidence_ref").HasMaxLength(500);
        });

        builder.Property(a => a.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(a => a.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(a => a.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(a => a.DomainEvents);

        builder.HasIndex(a => new { a.TenantId, a.EmployeeId })
            .HasDatabaseName("ix_training_assignments_tenant_employee");
        builder.HasIndex(a => new { a.TenantId, a.CourseId })
            .HasDatabaseName("ix_training_assignments_tenant_course");
        builder.HasIndex(a => new { a.TenantId, a.Status })
            .HasDatabaseName("ix_training_assignments_tenant_status");
        builder.HasIndex(a => new { a.TenantId, a.DueDate })
            .HasDatabaseName("ix_training_assignments_tenant_due");
    }
}
