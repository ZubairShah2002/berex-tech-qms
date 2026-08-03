using BerexQms.Domain.AuditManagement.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AuditManagement.Configurations;

public sealed class AuditChecklistConfiguration : IEntityTypeConfiguration<AuditChecklist>
{
    public void Configure(EntityTypeBuilder<AuditChecklist> builder)
    {
        builder.ToTable("audit_checklists", "audit");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(c => c.AuditRecordId)
            .HasColumnName("audit_record_id")
            .IsRequired();

        builder.Property(c => c.Standard)
            .HasColumnName("standard")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.ClauseReference)
            .HasColumnName("clause_reference")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Requirement)
            .HasColumnName("requirement")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(c => c.IsCompliant)
            .HasColumnName("is_compliant")
            .IsRequired();

        builder.Property(c => c.Evidence)
            .HasColumnName("evidence")
            .HasMaxLength(4000);

        builder.Property(c => c.Notes)
            .HasColumnName("notes")
            .HasMaxLength(2000);

        builder.HasIndex(c => c.AuditRecordId).HasDatabaseName("ix_audit_checklists_record_id");
        builder.HasIndex(c => c.TenantId).HasDatabaseName("ix_audit_checklists_tenant_id");
    }
}
