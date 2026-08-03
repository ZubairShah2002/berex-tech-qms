using BerexQms.Domain.AuditManagement.Entities;
using BerexQms.Domain.AuditManagement.Enums;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AuditManagement.Configurations;

public sealed class AuditFindingConfiguration : IEntityTypeConfiguration<AuditFinding>
{
    public void Configure(EntityTypeBuilder<AuditFinding> builder)
    {
        builder.ToTable("audit_findings", "audit");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(f => f.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(f => f.AuditRecordId)
            .HasColumnName("audit_record_id")
            .IsRequired();

        builder.Property(f => f.Classification)
            .HasColumnName("classification")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(f => f.ClauseReference)
            .HasColumnName("clause_reference")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.Description)
            .HasColumnName("description")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(f => f.Evidence)
            .HasColumnName("evidence")
            .HasMaxLength(4000);

        builder.Property(f => f.CorrectiveAction)
            .HasColumnName("corrective_action")
            .HasMaxLength(4000);

        builder.Property(f => f.LinkedCapaId)
            .HasColumnName("linked_capa_id")
            .HasMaxLength(200);

        builder.Property(f => f.FoundAt)
            .HasColumnName("found_at")
            .IsRequired();

        builder.HasIndex(f => f.AuditRecordId).HasDatabaseName("ix_audit_findings_record_id");
        builder.HasIndex(f => f.TenantId).HasDatabaseName("ix_audit_findings_tenant_id");
        builder.HasIndex(f => f.Classification).HasDatabaseName("ix_audit_findings_classification");
    }
}
