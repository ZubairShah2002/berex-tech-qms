using BerexQms.Domain.Inspection.Entities;
using BerexQms.Domain.ProductCatalog.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Inspection.Configurations;

public sealed class SamplingPlanConfiguration : IEntityTypeConfiguration<SamplingPlan>
{
    public void Configure(EntityTypeBuilder<SamplingPlan> builder)
    {
        builder.ToTable("sampling_plans", "inspection");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(s => s.PartId)
            .HasColumnName("part_id")
            .IsRequired();

        builder.Property(s => s.SupplierId)
            .HasColumnName("supplier_id");

        builder.Property(s => s.InspectionType)
            .HasColumnName("inspection_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.Level)
            .HasColumnName("level")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.AqlValue)
            .HasColumnName("aql_value")
            .HasPrecision(8, 4)
            .IsRequired();

        builder.Property(s => s.SampleSize)
            .HasColumnName("sample_size")
            .IsRequired();

        builder.Property(s => s.AcceptNumber)
            .HasColumnName("accept_number")
            .IsRequired();

        builder.Property(s => s.RejectNumber)
            .HasColumnName("reject_number")
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(s => s.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.ModifiedBy)
            .HasColumnName("modified_by")
            .HasMaxLength(100);

        builder.Property(s => s.ModifiedAt)
            .HasColumnName("modified_at");

        builder.HasOne<Part>()
            .WithMany()
            .HasForeignKey(s => s.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.TenantId).HasDatabaseName("ix_sampling_plans_tenant_id");
        builder.HasIndex(s => s.PartId).HasDatabaseName("ix_sampling_plans_part_id");
        builder.HasIndex(s => new { s.PartId, s.InspectionType, s.IsActive })
            .HasDatabaseName("ix_sampling_plans_part_type_active");

        builder.Ignore(s => s.DomainEvents);
    }
}
