using BerexQms.Domain.Inspection.Entities;
using BerexQms.Domain.Inspection.Enums;
using BerexQms.Domain.Inspection.ValueObjects;
using BerexQms.Domain.ProductCatalog.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Inspection.Configurations;

public sealed class InspectionRecordConfiguration : IEntityTypeConfiguration<InspectionRecord>
{
    public void Configure(EntityTypeBuilder<InspectionRecord> builder)
    {
        builder.ToTable("inspection_records", "inspection");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(r => r.InspectionNumber)
            .HasColumnName("inspection_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.PartId)
            .HasColumnName("part_id")
            .IsRequired();

        builder.Property(r => r.PartRevisionId)
            .HasColumnName("part_revision_id");

        builder.Property(r => r.LotNumber)
            .HasColumnName("lot_number")
            .HasMaxLength(100);

        builder.Property(r => r.LotSize)
            .HasColumnName("lot_size");

        builder.Property(r => r.SampleSize)
            .HasColumnName("sample_size");

        builder.Property(r => r.SupplierId)
            .HasColumnName("supplier_id");

        builder.Property(r => r.SamplingPlanId)
            .HasColumnName("sampling_plan_id");

        builder.Property(r => r.InspectorId)
            .HasColumnName("inspector_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Result)
            .HasColumnName("result")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.Property(r => r.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(r => r.CompletedBy)
            .HasColumnName("completed_by")
            .HasMaxLength(100);

        builder.Property(r => r.ApprovedAt)
            .HasColumnName("approved_at");

        builder.Property(r => r.ApprovedBy)
            .HasColumnName("approved_by")
            .HasMaxLength(100);

        builder.Property(r => r.RejectedAt)
            .HasColumnName("rejected_at");

        builder.Property(r => r.RejectedBy)
            .HasColumnName("rejected_by")
            .HasMaxLength(100);

        builder.Property(r => r.ChecklistId)
            .HasColumnName("checklist_id");

        builder.Property(r => r.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.ModifiedBy)
            .HasColumnName("modified_by")
            .HasMaxLength(100);

        builder.Property(r => r.ModifiedAt)
            .HasColumnName("modified_at");

        builder.OwnsOne(r => r.Disposition, d =>
        {
            d.Property(x => x.Type)
                .HasColumnName("disposition_type")
                .HasConversion<string>()
                .HasMaxLength(30);

            d.Property(x => x.Justification)
                .HasColumnName("disposition_justification")
                .HasMaxLength(2000);

            d.Property(x => x.ApprovedBy)
                .HasColumnName("disposition_approved_by")
                .HasMaxLength(100);

            d.Property(x => x.ApprovedAt)
                .HasColumnName("disposition_approved_at");
        });

        builder.OwnsMany(r => r.GateResults, g =>
        {
            g.ToTable("inspection_gate_results", "inspection");

            g.WithOwner().HasForeignKey("inspection_record_id");
            g.Property<int>("id").ValueGeneratedOnAdd();
            g.HasKey("id");

            g.Property<Guid>("tenant_id")
                .HasColumnName("tenant_id")
                .IsRequired()
                .HasDefaultValueSql("shared.current_tenant_id()");

            g.Property(x => x.GateType)
                .HasColumnName("gate_type")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            g.Property(x => x.Passed)
                .HasColumnName("passed")
                .IsRequired();

            g.Property(x => x.Detail)
                .HasColumnName("detail")
                .HasMaxLength(500);

            g.Property(x => x.CheckedAt)
                .HasColumnName("checked_at")
                .IsRequired();
        });

        builder.HasMany(r => r.Measurements)
            .WithOne()
            .HasForeignKey(m => m.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Checklist)
            .WithOne()
            .HasForeignKey<InspectionChecklist>(c => c.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Part>()
            .WithMany()
            .HasForeignKey(r => r.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.TenantId).HasDatabaseName("ix_inspection_records_tenant_id");
        builder.HasIndex(r => new { r.TenantId, r.InspectionNumber }).IsUnique()
            .HasDatabaseName("ix_inspection_records_tenant_number");
        builder.HasIndex(r => r.PartId).HasDatabaseName("ix_inspection_records_part_id");
        builder.HasIndex(r => r.Status).HasDatabaseName("ix_inspection_records_status");
        builder.HasIndex(r => r.Type).HasDatabaseName("ix_inspection_records_type");

        builder.Ignore(r => r.DomainEvents);
    }
}
