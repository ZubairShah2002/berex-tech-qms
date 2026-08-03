using BerexQms.Domain.DocumentControl.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.DocumentControl.Configurations;

public sealed class DistributionConfiguration : IEntityTypeConfiguration<Distribution>
{
    public void Configure(EntityTypeBuilder<Distribution> builder)
    {
        builder.ToTable("distributions", "document");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(d => d.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(d => d.DocumentVersionId)
            .HasColumnName("document_version_id")
            .IsRequired();

        builder.Property(d => d.RecipientId)
            .HasColumnName("recipient_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(d => d.DistributedAt)
            .HasColumnName("distributed_at")
            .IsRequired();

        builder.Property(d => d.AcknowledgedAt)
            .HasColumnName("acknowledged_at");

        builder.Property(d => d.ComplianceDeadline)
            .HasColumnName("compliance_deadline")
            .IsRequired();

        builder.HasIndex(d => d.DocumentVersionId).HasDatabaseName("ix_distributions_version_id");
        builder.HasIndex(d => d.TenantId).HasDatabaseName("ix_distributions_tenant_id");
        builder.HasIndex(d => d.RecipientId).HasDatabaseName("ix_distributions_recipient_id");

        builder.Ignore(d => d.IsAcknowledged);
        builder.Ignore(d => d.IsOverdue);
    }
}
