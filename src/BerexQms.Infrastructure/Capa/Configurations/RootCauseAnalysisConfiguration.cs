using BerexQms.Domain.Capa.Entities;
using BerexQms.Domain.Capa.Enums;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Capa.Configurations;

public sealed class RootCauseAnalysisConfiguration : IEntityTypeConfiguration<RootCauseAnalysis>
{
    public void Configure(EntityTypeBuilder<RootCauseAnalysis> builder)
    {
        builder.ToTable("root_cause_analyses", "capa");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(r => r.CapaId)
            .HasColumnName("capa_id")
            .IsRequired();

        builder.Property(r => r.Methodology)
            .HasColumnName("methodology")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(r => r.AnalysisDetails)
            .HasColumnName("analysis_details")
            .HasMaxLength(4000);

        builder.Property(r => r.RootCause)
            .HasColumnName("root_cause")
            .HasMaxLength(4000);

        builder.Property(r => r.ContributingFactors)
            .HasColumnName("contributing_factors")
            .HasMaxLength(4000);

        builder.Property(r => r.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(r => r.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(r => r.AnalystId)
            .HasColumnName("analyst_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(r => r.CapaId).HasDatabaseName("ix_rca_capa_id");
        builder.HasIndex(r => r.TenantId).HasDatabaseName("ix_rca_tenant_id");
    }
}
