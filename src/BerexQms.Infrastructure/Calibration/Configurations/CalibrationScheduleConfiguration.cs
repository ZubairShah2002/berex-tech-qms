using BerexQms.Domain.Calibration.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Calibration.Configurations;

public sealed class CalibrationScheduleConfiguration : IEntityTypeConfiguration<CalibrationSchedule>
{
    public void Configure(EntityTypeBuilder<CalibrationSchedule> builder)
    {
        builder.ToTable("schedules", "calibration");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(s => s.EquipmentId)
            .HasColumnName("equipment_id")
            .IsRequired();

        builder.Property(s => s.IntervalDays)
            .HasColumnName("interval_days")
            .IsRequired();

        builder.Property(s => s.LeadTimeDays)
            .HasColumnName("lead_time_days")
            .IsRequired();

        builder.Property(s => s.LabType)
            .HasColumnName("lab_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.ProcedureRef)
            .HasColumnName("procedure_ref")
            .HasMaxLength(200);

        builder.Property(s => s.NextDueDate)
            .HasColumnName("next_due_date")
            .IsRequired();

        builder.HasIndex(s => s.EquipmentId)
            .IsUnique()
            .HasDatabaseName("ix_schedules_equipment_id");
        builder.HasIndex(s => s.NextDueDate)
            .HasDatabaseName("ix_schedules_next_due");
        builder.HasIndex(s => s.TenantId)
            .HasDatabaseName("ix_schedules_tenant_id");
    }
}
