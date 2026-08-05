using BerexQms.Domain.Training.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Training.Configurations;

public sealed class TrainingCourseConfiguration : IEntityTypeConfiguration<TrainingCourse>
{
    public void Configure(EntityTypeBuilder<TrainingCourse> builder)
    {
        builder.ToTable("courses", "training");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(c => c.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(4000);

        builder.Property(c => c.DurationHours)
            .HasColumnName("duration_hours")
            .HasPrecision(6, 2)
            .IsRequired();

        builder.Property(c => c.AssessmentType)
            .HasColumnName("assessment_type")
            .HasMaxLength(100);

        builder.Property(c => c.PassCriteria)
            .HasColumnName("pass_criteria")
            .HasMaxLength(1000);

        builder.Property(c => c.QualificationId)
            .HasColumnName("qualification_id");

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(c => c.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(c => c.DomainEvents);

        builder.HasIndex(c => new { c.TenantId, c.Code })
            .IsUnique()
            .HasDatabaseName("ix_courses_tenant_code");
        builder.HasIndex(c => new { c.TenantId, c.QualificationId })
            .HasDatabaseName("ix_courses_tenant_qualification");
    }
}
