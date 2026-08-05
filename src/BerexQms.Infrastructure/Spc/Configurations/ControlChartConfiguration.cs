using BerexQms.Domain.Spc.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Spc.Configurations;

public sealed class ControlChartConfiguration : IEntityTypeConfiguration<ControlChart>
{
    public void Configure(EntityTypeBuilder<ControlChart> builder)
    {
        builder.ToTable("control_charts", "spc");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.Code).HasColumnName("code").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.ChartType).HasColumnName("chart_type").HasMaxLength(50).IsRequired();
        builder.Property(e => e.PartId).HasColumnName("part_id").IsRequired();
        builder.Property(e => e.CharacteristicName).HasColumnName("characteristic_name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.SubgroupSize).HasColumnName("subgroup_size").IsRequired();
        builder.Property(e => e.Status).HasColumnName("status").HasMaxLength(50).IsRequired();
        builder.Property(e => e.UpperSpecLimit).HasColumnName("upper_spec_limit").HasPrecision(18, 6);
        builder.Property(e => e.LowerSpecLimit).HasColumnName("lower_spec_limit").HasPrecision(18, 6);
        builder.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();

        // ControlLimits value object - flattened
        builder.OwnsOne(e => e.ControlLimits, cl =>
        {
            cl.Property(x => x.UpperControlLimit).HasColumnName("ucl").HasPrecision(18, 6);
            cl.Property(x => x.CenterLine).HasColumnName("center_line").HasPrecision(18, 6);
            cl.Property(x => x.LowerControlLimit).HasColumnName("lcl").HasPrecision(18, 6);
            cl.Property(x => x.UpperSpecLimit).HasColumnName("cl_upper_spec_limit").HasPrecision(18, 6);
            cl.Property(x => x.LowerSpecLimit).HasColumnName("cl_lower_spec_limit").HasPrecision(18, 6);
        });

        // ProcessCapability value object - flattened
        builder.OwnsOne(e => e.ProcessCapability, pc =>
        {
            pc.Property(x => x.Cp).HasColumnName("cp").HasPrecision(10, 4);
            pc.Property(x => x.Cpk).HasColumnName("cpk").HasPrecision(10, 4);
            pc.Property(x => x.Pp).HasColumnName("pp").HasPrecision(10, 4);
            pc.Property(x => x.Ppk).HasColumnName("ppk").HasPrecision(10, 4);
            pc.Property(x => x.Mean).HasColumnName("cap_mean").HasPrecision(18, 6);
            pc.Property(x => x.StdDev).HasColumnName("cap_std_dev").HasPrecision(18, 6);
            pc.Property(x => x.SampleSize).HasColumnName("cap_sample_size");
            pc.Property(x => x.CalculatedAt).HasColumnName("cap_calculated_at");
        });

        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

        builder.HasMany(e => e.DataPoints)
            .WithOne()
            .HasForeignKey(d => d.ControlChartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.DomainEvents);

        builder.HasIndex(e => new { e.TenantId, e.Code })
            .IsUnique()
            .HasDatabaseName("ix_control_charts_tenant_code");
        builder.HasIndex(e => new { e.TenantId, e.PartId })
            .HasDatabaseName("ix_control_charts_tenant_part");
        builder.HasIndex(e => new { e.TenantId, e.Status })
            .HasDatabaseName("ix_control_charts_tenant_status");
    }
}
