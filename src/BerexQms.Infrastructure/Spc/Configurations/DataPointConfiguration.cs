using BerexQms.Domain.Spc.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Spc.Configurations;

public sealed class DataPointConfiguration : IEntityTypeConfiguration<DataPoint>
{
    public void Configure(EntityTypeBuilder<DataPoint> builder)
    {
        builder.ToTable("data_points", "spc");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.ControlChartId).HasColumnName("control_chart_id").IsRequired();
        builder.Property(e => e.Value).HasColumnName("value").HasPrecision(18, 6).IsRequired();
        builder.Property(e => e.SubgroupValues).HasColumnName("subgroup_values").HasMaxLength(2000);
        builder.Property(e => e.SampleSize).HasColumnName("sample_size").IsRequired();
        builder.Property(e => e.Timestamp).HasColumnName("timestamp").IsRequired();
        builder.Property(e => e.InspectionId).HasColumnName("inspection_id");
        builder.Property(e => e.RuleViolation).HasColumnName("rule_violation").HasMaxLength(100);
        builder.Property(e => e.IsOutOfControl).HasColumnName("is_out_of_control").IsRequired();

        builder.HasIndex(e => new { e.ControlChartId, e.Timestamp })
            .HasDatabaseName("ix_data_points_chart_timestamp");
        builder.HasIndex(e => new { e.TenantId, e.InspectionId })
            .HasDatabaseName("ix_data_points_tenant_inspection");
    }
}
