using BerexQms.Domain.Calibration.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Calibration.Configurations;

public sealed class CalibrationRecordConfiguration : IEntityTypeConfiguration<CalibrationRecord>
{
    public void Configure(EntityTypeBuilder<CalibrationRecord> builder)
    {
        builder.ToTable("calibration_records", "calibration");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(c => c.EquipmentId)
            .HasColumnName("equipment_id")
            .IsRequired();

        builder.Property(c => c.CalibrationDate)
            .HasColumnName("calibration_date")
            .IsRequired();

        builder.Property(c => c.Result)
            .HasColumnName("result")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.TechnicianId)
            .HasColumnName("technician_id");

        builder.Property(c => c.ProcedureRef)
            .HasColumnName("procedure_ref")
            .HasMaxLength(200);

        builder.Property(c => c.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.Property(c => c.EnvironmentalConditions)
            .HasColumnName("environmental_conditions")
            .HasMaxLength(500);

        builder.Property(c => c.NextDueDate)
            .HasColumnName("next_due_date");

        builder.OwnsOne(c => c.Certificate, cert =>
        {
            cert.Property(x => x.IssuingLab).HasColumnName("cert_issuing_lab").HasMaxLength(200);
            cert.Property(x => x.AccreditationRef).HasColumnName("cert_accreditation_ref").HasMaxLength(200);
            cert.Property(x => x.FileRef).HasColumnName("cert_file_ref").HasMaxLength(500);
            cert.Property(x => x.ValidFrom).HasColumnName("cert_valid_from");
            cert.Property(x => x.ValidUntil).HasColumnName("cert_valid_until");
        });

        builder.HasIndex(c => new { c.EquipmentId, c.CalibrationDate })
            .HasDatabaseName("ix_cal_records_equip_date");
        builder.HasIndex(c => c.NextDueDate)
            .HasDatabaseName("ix_cal_records_next_due");
        builder.HasIndex(c => c.TenantId)
            .HasDatabaseName("ix_cal_records_tenant_id");
    }
}
