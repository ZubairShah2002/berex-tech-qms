using BerexQms.Domain.Inspection.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Inspection.Configurations;

public sealed class ChecklistItemConfiguration : IEntityTypeConfiguration<ChecklistItem>
{
    public void Configure(EntityTypeBuilder<ChecklistItem> builder)
    {
        builder.ToTable("checklist_items", "inspection");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(i => i.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(i => i.ChecklistId)
            .HasColumnName("checklist_id")
            .IsRequired();

        builder.Property(i => i.CharacteristicName)
            .HasColumnName("characteristic_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.SpecificationLimit)
            .HasColumnName("specification_limit")
            .HasMaxLength(200);

        builder.Property(i => i.NominalValue)
            .HasColumnName("nominal_value")
            .HasPrecision(18, 6);

        builder.Property(i => i.UpperLimit)
            .HasColumnName("upper_limit")
            .HasPrecision(18, 6);

        builder.Property(i => i.LowerLimit)
            .HasColumnName("lower_limit")
            .HasPrecision(18, 6);

        builder.Property(i => i.Unit)
            .HasColumnName("unit")
            .HasMaxLength(20);

        builder.Property(i => i.IsCritical)
            .HasColumnName("is_critical")
            .IsRequired();

        builder.Property(i => i.SortOrder)
            .HasColumnName("sort_order")
            .IsRequired();

        builder.HasIndex(i => i.ChecklistId).HasDatabaseName("ix_checklist_items_checklist_id");
    }
}
