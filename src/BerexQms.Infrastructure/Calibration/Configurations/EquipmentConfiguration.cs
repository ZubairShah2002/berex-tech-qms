using BerexQms.Domain.Calibration.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Calibration.Configurations;

public sealed class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("equipment", "calibration");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.Code)
            .HasColumnName("code")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Type)
            .HasColumnName("type")
            .HasMaxLength(100);

        builder.Property(e => e.Manufacturer)
            .HasColumnName("manufacturer")
            .HasMaxLength(200);

        builder.Property(e => e.Model)
            .HasColumnName("model")
            .HasMaxLength(200);

        builder.Property(e => e.SerialNumber)
            .HasColumnName("serial_number")
            .HasMaxLength(100);

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Location)
            .HasColumnName("location")
            .HasMaxLength(200);

        builder.OwnsOne(e => e.Assignment, a =>
        {
            a.Property(x => x.Department).HasColumnName("department").HasMaxLength(200);
            a.Property(x => x.Area).HasColumnName("area").HasMaxLength(200);
            a.Property(x => x.CustodianId).HasColumnName("custodian_id");
        });

        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

        builder.HasOne(e => e.Schedule)
            .WithOne()
            .HasForeignKey<CalibrationSchedule>(s => s.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Calibrations)
            .WithOne()
            .HasForeignKey(c => c.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.GaugeStudies)
            .WithOne()
            .HasForeignKey(g => g.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ImpactAssessments)
            .WithOne()
            .HasForeignKey(ia => ia.EquipmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.DomainEvents);

        builder.HasIndex(e => new { e.TenantId, e.Code })
            .IsUnique()
            .HasDatabaseName("ix_equipment_tenant_code");
        builder.HasIndex(e => new { e.TenantId, e.Status })
            .HasDatabaseName("ix_equipment_tenant_status");
    }
}
