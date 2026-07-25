using BerexQms.Domain.ProductCatalog.Entities;
using BerexQms.Domain.ProductCatalog.Enums;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.ProductCatalog.Configurations;

public sealed class SpecificationParameterConfiguration : IEntityTypeConfiguration<SpecificationParameter>
{
    public void Configure(EntityTypeBuilder<SpecificationParameter> builder)
    {
        builder.ToTable("specification_parameters", "catalog");

        builder.HasKey(sp => sp.Id);
        builder.Property(sp => sp.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(sp => sp.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(sp => sp.PartRevisionId)
            .HasColumnName("part_revision_id")
            .IsRequired();

        builder.Property(sp => sp.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(sp => sp.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(sp => sp.Unit)
            .HasColumnName("unit")
            .HasMaxLength(50);

        builder.Property(sp => sp.NominalValue)
            .HasColumnName("nominal_value")
            .HasPrecision(18, 6);

        builder.Property(sp => sp.UpperTolerance)
            .HasColumnName("upper_tolerance")
            .HasPrecision(18, 6);

        builder.Property(sp => sp.LowerTolerance)
            .HasColumnName("lower_tolerance")
            .HasPrecision(18, 6);

        builder.Property(sp => sp.TextValue)
            .HasColumnName("text_value")
            .HasMaxLength(500);

        builder.Property(sp => sp.IsCritical)
            .HasColumnName("is_critical")
            .IsRequired();

        builder.Property(sp => sp.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.HasIndex(sp => sp.TenantId).HasDatabaseName("ix_spec_params_tenant_id");
        builder.HasIndex(sp => sp.PartRevisionId).HasDatabaseName("ix_spec_params_revision_id");
    }
}
