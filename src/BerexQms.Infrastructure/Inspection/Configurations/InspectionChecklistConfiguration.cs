using BerexQms.Domain.Inspection.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Inspection.Configurations;

public sealed class InspectionChecklistConfiguration : IEntityTypeConfiguration<InspectionChecklist>
{
    public void Configure(EntityTypeBuilder<InspectionChecklist> builder)
    {
        builder.ToTable("inspection_checklists", "inspection");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(c => c.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(c => c.InspectionId)
            .HasColumnName("inspection_id")
            .IsRequired();

        builder.Property(c => c.PartRevisionId)
            .HasColumnName("part_revision_id")
            .IsRequired();

        builder.Property(c => c.RevisionCode)
            .HasColumnName("revision_code")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.SnapshotAt)
            .HasColumnName("snapshot_at")
            .IsRequired();

        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey(i => i.ChecklistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.InspectionId).IsUnique()
            .HasDatabaseName("ix_inspection_checklists_inspection_id");
    }
}
