using BerexQms.Domain.NonConformance.Entities;
using BerexQms.Domain.NonConformance.Enums;
using BerexQms.Domain.ProductCatalog.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.NonConformance.Configurations;

public sealed class NonConformanceRecordConfiguration : IEntityTypeConfiguration<NonConformanceRecord>
{
    public void Configure(EntityTypeBuilder<NonConformanceRecord> builder)
    {
        builder.ToTable("non_conformance_records", "ncr");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(r => r.NcrNumber)
            .HasColumnName("ncr_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.Severity)
            .HasColumnName("severity")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(r => r.Source)
            .HasColumnName("source")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.DetectionPoint)
            .HasColumnName("detection_point")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(r => r.PartId)
            .HasColumnName("part_id")
            .IsRequired();

        builder.Property(r => r.PartRevisionId)
            .HasColumnName("part_revision_id");

        builder.Property(r => r.LotNumber)
            .HasColumnName("lot_number")
            .HasMaxLength(100);

        builder.Property(r => r.SerialNumber)
            .HasColumnName("serial_number")
            .HasMaxLength(100);

        builder.Property(r => r.SupplierId)
            .HasColumnName("supplier_id");

        builder.Property(r => r.SupplierLotNumber)
            .HasColumnName("supplier_lot_number")
            .HasMaxLength(100);

        builder.Property(r => r.WorkOrderNumber)
            .HasColumnName("work_order_number")
            .HasMaxLength(100);

        builder.Property(r => r.CustomerId)
            .HasColumnName("customer_id");

        builder.Property(r => r.SourceInspectionId)
            .HasColumnName("source_inspection_id");

        builder.Property(r => r.QuantityAffected)
            .HasColumnName("quantity_affected")
            .IsRequired();

        builder.Property(r => r.QuantityDefective)
            .HasColumnName("quantity_defective")
            .IsRequired();

        builder.Property(r => r.AssignedTo)
            .HasColumnName("assigned_to")
            .HasMaxLength(100);

        builder.Property(r => r.CapaId)
            .HasColumnName("capa_id");

        builder.Property(r => r.ClosedAt)
            .HasColumnName("closed_at");

        builder.Property(r => r.ClosedBy)
            .HasColumnName("closed_by")
            .HasMaxLength(100);

        builder.Property(r => r.ReopenedAt)
            .HasColumnName("reopened_at");

        builder.Property(r => r.ReopenedBy)
            .HasColumnName("reopened_by")
            .HasMaxLength(100);

        builder.Property(r => r.ReopenReason)
            .HasColumnName("reopen_reason")
            .HasMaxLength(4000);

        builder.Property(r => r.ClosureNotes)
            .HasColumnName("closure_notes")
            .HasMaxLength(4000);

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

        builder.OwnsOne(r => r.Classification, c =>
        {
            c.Property(x => x.Category)
                .HasColumnName("classification_category")
                .HasMaxLength(200);

            c.Property(x => x.DefectType)
                .HasColumnName("classification_defect_type")
                .HasMaxLength(200);

            c.Property(x => x.DefectCode)
                .HasColumnName("classification_defect_code")
                .HasMaxLength(50);
        });

        builder.OwnsOne(r => r.Disposition, d =>
        {
            d.Property(x => x.Type)
                .HasColumnName("disposition_type")
                .HasConversion<string>()
                .HasMaxLength(30);

            d.Property(x => x.Justification)
                .HasColumnName("disposition_justification")
                .HasMaxLength(4000);

            d.Property(x => x.ApprovedBy)
                .HasColumnName("disposition_approved_by")
                .HasMaxLength(100);

            d.Property(x => x.ApprovedAt)
                .HasColumnName("disposition_approved_at");
        });

        builder.OwnsOne(r => r.ImpactAssessment, ia =>
        {
            ia.Property(x => x.AffectedQuantity)
                .HasColumnName("impact_affected_quantity");

            ia.Property(x => x.ShippedProductAffected)
                .HasColumnName("impact_shipped_product_affected");

            ia.Property(x => x.CustomerImpactDescription)
                .HasColumnName("impact_customer_description")
                .HasMaxLength(2000);
        });

        builder.HasMany(r => r.ContainmentActions)
            .WithOne()
            .HasForeignKey(a => a.NonConformanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Investigations)
            .WithOne()
            .HasForeignKey(i => i.NonConformanceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Part>()
            .WithMany()
            .HasForeignKey(r => r.PartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.TenantId).HasDatabaseName("ix_ncr_records_tenant_id");
        builder.HasIndex(r => new { r.TenantId, r.NcrNumber }).IsUnique()
            .HasDatabaseName("ix_ncr_records_tenant_number");
        builder.HasIndex(r => r.PartId).HasDatabaseName("ix_ncr_records_part_id");
        builder.HasIndex(r => r.Status).HasDatabaseName("ix_ncr_records_status");
        builder.HasIndex(r => r.Severity).HasDatabaseName("ix_ncr_records_severity");
        builder.HasIndex(r => r.SupplierId).HasDatabaseName("ix_ncr_records_supplier_id");
        builder.HasIndex(r => r.CreatedAt).HasDatabaseName("ix_ncr_records_created_at");

        builder.Ignore(r => r.DomainEvents);
    }
}
