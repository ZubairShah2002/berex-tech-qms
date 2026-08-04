using BerexQms.Domain.SupplierQuality.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.SupplierQuality.Configurations;

public sealed class SCARRecordConfiguration : IEntityTypeConfiguration<SCARRecord>
{
    public void Configure(EntityTypeBuilder<SCARRecord> builder)
    {
        builder.ToTable("scar_records", "supplier");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(s => s.SupplierId)
            .HasColumnName("supplier_id")
            .IsRequired();

        builder.Property(s => s.ScarNumber)
            .HasColumnName("scar_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.NonConformanceId)
            .HasColumnName("nc_id");

        builder.Property(s => s.DefectDescription)
            .HasColumnName("defect_description")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(s => s.Severity)
            .HasColumnName("severity")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.IssuedDate)
            .HasColumnName("issued_date")
            .IsRequired();

        builder.Property(s => s.ResponseDeadline)
            .HasColumnName("response_deadline")
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.OwnsOne(s => s.Response, r =>
        {
            r.Property(x => x.RootCause)
                .HasColumnName("response_root_cause")
                .HasMaxLength(4000);

            r.Property(x => x.CorrectiveActions)
                .HasColumnName("response_corrective_actions")
                .HasMaxLength(4000);

            r.Property(x => x.EvidenceRefs)
                .HasColumnName("response_evidence_refs")
                .HasMaxLength(4000);

            r.Property(x => x.ResponseDate)
                .HasColumnName("response_date");
        });

        builder.HasIndex(s => new { s.SupplierId, s.Status })
            .HasDatabaseName("ix_scar_records_supplier_status");
        builder.HasIndex(s => s.NonConformanceId)
            .HasDatabaseName("ix_scar_records_nc_id");
        builder.HasIndex(s => s.TenantId)
            .HasDatabaseName("ix_scar_records_tenant_id");
    }
}
