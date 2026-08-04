using BerexQms.Domain.Calibration.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Calibration.Configurations;

public sealed class GaugeControlConfiguration : IEntityTypeConfiguration<GaugeControl>
{
    public void Configure(EntityTypeBuilder<GaugeControl> builder)
    {
        builder.ToTable("gauge_rr_studies", "calibration");

        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(g => g.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(g => g.EquipmentId)
            .HasColumnName("equipment_id")
            .IsRequired();

        builder.Property(g => g.CharacteristicId)
            .HasColumnName("characteristic_id");

        builder.Property(g => g.StudyDate)
            .HasColumnName("study_date")
            .IsRequired();

        builder.Property(g => g.TotalGRRPct)
            .HasColumnName("total_grr_pct")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(g => g.RepeatabilityPct)
            .HasColumnName("repeatability_pct")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(g => g.ReproducibilityPct)
            .HasColumnName("reproducibility_pct")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(g => g.PartVariationPct)
            .HasColumnName("part_variation_pct")
            .HasPrecision(5, 2);

        builder.Property(g => g.Ndc)
            .HasColumnName("ndc");

        builder.Property(g => g.Result)
            .HasColumnName("result")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(g => new { g.EquipmentId, g.CharacteristicId })
            .HasDatabaseName("ix_gauge_rr_equip_char");
        builder.HasIndex(g => g.TenantId)
            .HasDatabaseName("ix_gauge_rr_tenant_id");
    }
}
