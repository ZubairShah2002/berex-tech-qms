using BerexQms.Domain.Capa.Entities;
using BerexQms.Domain.Capa.Enums;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Capa.Configurations;

public sealed class CAPARecordConfiguration : IEntityTypeConfiguration<CAPARecord>
{
    public void Configure(EntityTypeBuilder<CAPARecord> builder)
    {
        builder.ToTable("capa_records", "capa");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(r => r.CapaNumber)
            .HasColumnName("capa_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.Priority)
            .HasColumnName("priority")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.OwnsOne(r => r.Source, s =>
        {
            s.Property(x => x.SourceType)
                .HasColumnName("source_type")
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            s.Property(x => x.SourceNonConformanceId)
                .HasColumnName("source_non_conformance_id");

            s.Property(x => x.SourceAuditFindingId)
                .HasColumnName("source_audit_finding_id");

            s.Property(x => x.SourceDescription)
                .HasColumnName("source_description")
                .HasMaxLength(4000);
        });

        builder.Property(r => r.OwnerId)
            .HasColumnName("owner_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.AssignedTo)
            .HasColumnName("assigned_to")
            .HasMaxLength(100);

        builder.Property(r => r.SourceNonConformanceId)
            .HasColumnName("source_nc_id");

        builder.Property(r => r.RootCauseAnalysisId)
            .HasColumnName("root_cause_analysis_id");

        builder.HasOne(r => r.RootCauseAnalysis)
            .WithOne()
            .HasForeignKey<CAPARecord>(r => r.RootCauseAnalysisId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(r => r.TargetClosureDate)
            .HasColumnName("target_closure_date");

        builder.Property(r => r.ClosedAt)
            .HasColumnName("closed_at");

        builder.Property(r => r.ClosedBy)
            .HasColumnName("closed_by")
            .HasMaxLength(100);

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

        builder.HasMany(r => r.Actions)
            .WithOne()
            .HasForeignKey(a => a.CapaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Verifications)
            .WithOne()
            .HasForeignKey(v => v.CapaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.TenantId).HasDatabaseName("ix_capa_records_tenant_id");
        builder.HasIndex(r => new { r.TenantId, r.CapaNumber }).IsUnique()
            .HasDatabaseName("ix_capa_records_tenant_number");
        builder.HasIndex(r => r.Status).HasDatabaseName("ix_capa_records_status");
        builder.HasIndex(r => r.Priority).HasDatabaseName("ix_capa_records_priority");
        builder.HasIndex(r => r.CreatedAt).HasDatabaseName("ix_capa_records_created_at");

        builder.Ignore(r => r.DomainEvents);
    }
}
