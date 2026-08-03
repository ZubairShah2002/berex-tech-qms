using BerexQms.Domain.Capa.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Capa.Configurations;

public sealed class EffectivenessVerificationConfiguration : IEntityTypeConfiguration<EffectivenessVerification>
{
    public void Configure(EntityTypeBuilder<EffectivenessVerification> builder)
    {
        builder.ToTable("effectiveness_verifications", "capa");

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(v => v.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(v => v.CapaId)
            .HasColumnName("capa_id")
            .IsRequired();

        builder.Property(v => v.ScheduledDate)
            .HasColumnName("scheduled_date")
            .IsRequired();

        builder.Property(v => v.VerificationCriteria)
            .HasColumnName("verification_criteria")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(v => v.VerifierId)
            .HasColumnName("verifier_id")
            .HasMaxLength(100);

        builder.Property(v => v.Result)
            .HasColumnName("result")
            .HasMaxLength(4000);

        builder.Property(v => v.Evidence)
            .HasColumnName("evidence")
            .HasMaxLength(4000);

        builder.Property(v => v.IsEffective)
            .HasColumnName("is_effective");

        builder.Property(v => v.VerifiedAt)
            .HasColumnName("verified_at");

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(v => v.CapaId).HasDatabaseName("ix_effectiveness_verifications_capa_id");
        builder.HasIndex(v => v.TenantId).HasDatabaseName("ix_effectiveness_verifications_tenant_id");
    }
}
