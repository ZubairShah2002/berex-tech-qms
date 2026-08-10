using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AiEngine.Configurations;

public sealed class AiPromptTemplateConfiguration : IEntityTypeConfiguration<AiPromptTemplate>
{
    public void Configure(EntityTypeBuilder<AiPromptTemplate> builder)
    {
        builder.ToTable("ai_prompt_templates", "ai_engine");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(v => v.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.TaskType).HasColumnName("task_type").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(e => e.SystemPrompt).HasColumnName("system_prompt").IsRequired();
        builder.Property(e => e.UserPromptTemplate).HasColumnName("user_prompt_template").IsRequired();
        builder.Property(e => e.OutputSchema).HasColumnName("output_schema");
        builder.Property(e => e.IsActive).HasColumnName("is_active");
        builder.Property(e => e.Version).HasColumnName("version");

        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(256);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(256);
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

        builder.HasIndex(e => new { e.TenantId, e.TaskType, e.IsActive })
            .HasDatabaseName("ix_ai_prompt_templates_tenant_task_active");
    }
}
