using BerexQms.Domain.SupplierQuality.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.SupplierQuality.Configurations;

public sealed class SupplierScorecardConfiguration : IEntityTypeConfiguration<SupplierScorecard>
{
    public void Configure(EntityTypeBuilder<SupplierScorecard> builder)
    {
        builder.ToTable("scorecards", "supplier");

        builder.HasKey(sc => sc.Id);
        builder.Property(sc => sc.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(sc => sc.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(sc => sc.SupplierId)
            .HasColumnName("supplier_id")
            .IsRequired();

        builder.Property(sc => sc.PeriodStart)
            .HasColumnName("period_start")
            .IsRequired();

        builder.Property(sc => sc.PeriodEnd)
            .HasColumnName("period_end")
            .IsRequired();

        builder.Property(sc => sc.QualityScore)
            .HasColumnName("quality_score")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(sc => sc.DeliveryScore)
            .HasColumnName("delivery_score")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(sc => sc.ResponsivenessScore)
            .HasColumnName("responsiveness_score")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(sc => sc.CostScore)
            .HasColumnName("cost_score")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(sc => sc.OverallScore)
            .HasColumnName("overall_score")
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(sc => sc.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(sc => new { sc.SupplierId, sc.PeriodStart })
            .IsUnique()
            .HasDatabaseName("ix_scorecards_supplier_period");
        builder.HasIndex(sc => sc.TenantId)
            .HasDatabaseName("ix_scorecards_tenant_id");
    }
}
