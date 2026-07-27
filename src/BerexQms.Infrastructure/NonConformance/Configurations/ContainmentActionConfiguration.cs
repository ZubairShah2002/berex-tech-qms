using BerexQms.Domain.NonConformance.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.NonConformance.Configurations;

public sealed class ContainmentActionConfiguration : IEntityTypeConfiguration<ContainmentAction>
{
    public void Configure(EntityTypeBuilder<ContainmentAction> builder)
    {
        builder.ToTable("containment_actions", "ncr");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(a => a.NonConformanceId)
            .HasColumnName("non_conformance_id")
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(a => a.ActionTakenBy)
            .HasColumnName("action_taken_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.ActionTakenAt)
            .HasColumnName("action_taken_at")
            .IsRequired();

        builder.Property(a => a.IsVerified)
            .HasColumnName("is_verified")
            .IsRequired();

        builder.Property(a => a.VerifiedBy)
            .HasColumnName("verified_by")
            .HasMaxLength(100);

        builder.Property(a => a.VerifiedAt)
            .HasColumnName("verified_at");

        builder.HasIndex(a => a.NonConformanceId).HasDatabaseName("ix_containment_actions_nc_id");
        builder.HasIndex(a => a.TenantId).HasDatabaseName("ix_containment_actions_tenant_id");
    }
}
