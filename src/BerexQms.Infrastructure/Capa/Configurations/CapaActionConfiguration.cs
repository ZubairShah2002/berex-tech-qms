using BerexQms.Domain.Capa.Entities;
using BerexQms.Domain.Capa.Enums;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Capa.Configurations;

public sealed class CapaActionConfiguration : IEntityTypeConfiguration<CapaAction>
{
    public void Configure(EntityTypeBuilder<CapaAction> builder)
    {
        builder.ToTable("capa_actions", "capa");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(a => a.CapaId)
            .HasColumnName("capa_id")
            .IsRequired();

        builder.Property(a => a.ActionType)
            .HasColumnName("action_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(a => a.OwnerId)
            .HasColumnName("owner_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.DueDate)
            .HasColumnName("due_date")
            .IsRequired();

        builder.Property(a => a.EvidenceRequirement)
            .HasColumnName("evidence_requirement")
            .HasMaxLength(2000);

        builder.Property(a => a.CompletionNotes)
            .HasColumnName("completion_notes")
            .HasMaxLength(4000);

        builder.Property(a => a.EvidenceProvided)
            .HasColumnName("evidence_provided")
            .HasMaxLength(4000);

        builder.Property(a => a.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(a => a.CompletedBy)
            .HasColumnName("completed_by")
            .HasMaxLength(100);

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(a => a.CapaId).HasDatabaseName("ix_capa_actions_capa_id");
        builder.HasIndex(a => a.TenantId).HasDatabaseName("ix_capa_actions_tenant_id");
        builder.HasIndex(a => a.DueDate).HasDatabaseName("ix_capa_actions_due_date");

        builder.Ignore(a => a.IsOverdue);
    }
}
