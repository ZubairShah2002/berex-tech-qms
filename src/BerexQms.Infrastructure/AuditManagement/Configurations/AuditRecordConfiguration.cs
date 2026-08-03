using BerexQms.Domain.AuditManagement.Entities;
using BerexQms.Domain.AuditManagement.Enums;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AuditManagement.Configurations;

public sealed class AuditRecordConfiguration : IEntityTypeConfiguration<AuditRecord>
{
    public void Configure(EntityTypeBuilder<AuditRecord> builder)
    {
        builder.ToTable("audit_records", "audit");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(r => r.AuditPlanId)
            .HasColumnName("audit_plan_id")
            .IsRequired();

        builder.Property(r => r.AuditNumber)
            .HasColumnName("audit_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.AuditType)
            .HasColumnName("audit_type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.LeadAuditorId)
            .HasColumnName("lead_auditor_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.AuditeeArea)
            .HasColumnName("auditee_area")
            .HasMaxLength(200);

        builder.Property(r => r.ScheduledDate)
            .HasColumnName("scheduled_date")
            .IsRequired();

        builder.Property(r => r.StartedAt)
            .HasColumnName("started_at");

        builder.Property(r => r.CompletedAt)
            .HasColumnName("completed_at");

        builder.OwnsOne(r => r.Report, report =>
        {
            report.Property(x => x.Summary)
                .HasColumnName("report_summary")
                .HasMaxLength(4000);

            report.Property(x => x.Recommendations)
                .HasColumnName("report_recommendations")
                .HasMaxLength(4000);

            report.Property(x => x.AuditorNotes)
                .HasColumnName("report_auditor_notes")
                .HasMaxLength(4000);

            report.Property(x => x.GeneratedAt)
                .HasColumnName("report_generated_at");
        });

        builder.HasMany(r => r.Findings)
            .WithOne()
            .HasForeignKey(f => f.AuditRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Checklists)
            .WithOne()
            .HasForeignKey(c => c.AuditRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.AuditPlanId).HasDatabaseName("ix_audit_records_plan_id");
        builder.HasIndex(r => r.TenantId).HasDatabaseName("ix_audit_records_tenant_id");
        builder.HasIndex(r => r.Status).HasDatabaseName("ix_audit_records_status");
        builder.HasIndex(r => r.ScheduledDate).HasDatabaseName("ix_audit_records_scheduled_date");
    }
}
