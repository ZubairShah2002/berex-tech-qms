using BerexQms.Domain.Calibration.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Calibration.Configurations;

public sealed class ImpactAssessmentConfiguration : IEntityTypeConfiguration<ImpactAssessment>
{
    public void Configure(EntityTypeBuilder<ImpactAssessment> builder)
    {
        builder.ToTable("impact_assessments", "calibration");

        builder.HasKey(ia => ia.Id);
        builder.Property(ia => ia.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(ia => ia.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(ia => ia.EquipmentId)
            .HasColumnName("equipment_id")
            .IsRequired();

        builder.Property(ia => ia.FailedCalibrationId)
            .HasColumnName("failed_cal_id")
            .IsRequired();

        builder.Property(ia => ia.AffectedFrom)
            .HasColumnName("affected_from")
            .IsRequired();

        builder.Property(ia => ia.AffectedTo)
            .HasColumnName("affected_to")
            .IsRequired();

        builder.Property(ia => ia.AffectedInspectionCount)
            .HasColumnName("affected_inspection_count")
            .IsRequired();

        builder.Property(ia => ia.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ia => ia.ReviewedBy)
            .HasColumnName("reviewed_by");

        builder.Property(ia => ia.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.HasIndex(ia => new { ia.EquipmentId, ia.FailedCalibrationId })
            .HasDatabaseName("ix_impact_equip_cal");
        builder.HasIndex(ia => ia.TenantId)
            .HasDatabaseName("ix_impact_tenant_id");
    }
}
