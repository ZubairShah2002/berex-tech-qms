using BerexQms.Domain.NonConformance.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.NonConformance.Configurations;

public sealed class InvestigationConfiguration : IEntityTypeConfiguration<Investigation>
{
    public void Configure(EntityTypeBuilder<Investigation> builder)
    {
        builder.ToTable("investigations", "ncr");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(i => i.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(i => i.NonConformanceId)
            .HasColumnName("non_conformance_id")
            .IsRequired();

        builder.Property(i => i.InvestigatorId)
            .HasColumnName("investigator_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.Methodology)
            .HasColumnName("methodology")
            .HasMaxLength(200);

        builder.Property(i => i.RootCause)
            .HasColumnName("root_cause")
            .HasMaxLength(4000);

        builder.Property(i => i.Findings)
            .HasColumnName("findings")
            .HasMaxLength(4000);

        builder.Property(i => i.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(i => i.CompletedAt)
            .HasColumnName("completed_at");

        builder.HasIndex(i => i.NonConformanceId).HasDatabaseName("ix_investigations_nc_id");
        builder.HasIndex(i => i.TenantId).HasDatabaseName("ix_investigations_tenant_id");
    }
}
