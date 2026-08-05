using BerexQms.Domain.Training.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Training.Configurations;

public sealed class CompetencyRecordConfiguration : IEntityTypeConfiguration<CompetencyRecord>
{
    public void Configure(EntityTypeBuilder<CompetencyRecord> builder)
    {
        builder.ToTable("competency_records", "training");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(r => r.EmployeeId)
            .HasColumnName("employee_id")
            .IsRequired();

        builder.Property(r => r.QualificationId)
            .HasColumnName("qualification_id")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.QualifiedDate)
            .HasColumnName("qualified_date");

        builder.Property(r => r.ExpiryDate)
            .HasColumnName("expiry_date");

        builder.Property(r => r.AssessorId)
            .HasColumnName("assessor_id");

        builder.Property(r => r.EvidenceRef)
            .HasColumnName("evidence_ref")
            .HasMaxLength(500);

        builder.HasIndex(r => new { r.TenantId, r.EmployeeId, r.QualificationId })
            .IsUnique()
            .HasDatabaseName("ix_competency_records_tenant_employee_qualification");
        builder.HasIndex(r => new { r.TenantId, r.Status })
            .HasDatabaseName("ix_competency_records_tenant_status");
        builder.HasIndex(r => new { r.TenantId, r.ExpiryDate })
            .HasDatabaseName("ix_competency_records_tenant_expiry");
    }
}
