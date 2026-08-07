using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AiEngine.Configurations;

public sealed class AiWorkflowDefinitionConfiguration : IEntityTypeConfiguration<AiWorkflowDefinition>
{
    public void Configure(EntityTypeBuilder<AiWorkflowDefinition> builder)
    {
        builder.ToTable("ai_workflow_definitions", "ai_engine");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .HasConversion(v => v.Value, v => TenantId.From(v))
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(x => x.MinimumPermissionLevel).HasColumnName("minimum_permission_level").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Category).HasColumnName("category").HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(x => x.StepsDefinition).HasColumnName("steps_definition").HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.AffectedModules).HasColumnName("affected_modules").HasMaxLength(500).IsRequired();

        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(x => x.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique()
            .HasDatabaseName("ix_ai_workflow_definitions_tenant_name");
    }
}
