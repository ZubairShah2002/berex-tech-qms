using BerexQms.Domain.Inspection.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Inspection.Configurations;

public sealed class MeasurementConfiguration : IEntityTypeConfiguration<Measurement>
{
    public void Configure(EntityTypeBuilder<Measurement> builder)
    {
        builder.ToTable("measurements", "inspection");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(m => m.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(m => m.InspectionId)
            .HasColumnName("inspection_id")
            .IsRequired();

        builder.Property(m => m.ChecklistItemId)
            .HasColumnName("checklist_item_id");

        builder.Property(m => m.CharacteristicName)
            .HasColumnName("characteristic_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.MeasuredValue)
            .HasColumnName("measured_value")
            .HasPrecision(18, 6);

        builder.Property(m => m.TextValue)
            .HasColumnName("text_value")
            .HasMaxLength(500);

        builder.Property(m => m.Unit)
            .HasColumnName("unit")
            .HasMaxLength(20);

        builder.Property(m => m.Result)
            .HasColumnName("result")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(m => m.EquipmentId)
            .HasColumnName("equipment_id");

        builder.Property(m => m.OperatorId)
            .HasColumnName("operator_id")
            .HasMaxLength(100);

        builder.Property(m => m.RecordedAt)
            .HasColumnName("recorded_at")
            .IsRequired();

        builder.Property(m => m.SequenceNumber)
            .HasColumnName("sequence_number")
            .IsRequired();

        builder.HasIndex(m => m.InspectionId).HasDatabaseName("ix_measurements_inspection_id");
    }
}
