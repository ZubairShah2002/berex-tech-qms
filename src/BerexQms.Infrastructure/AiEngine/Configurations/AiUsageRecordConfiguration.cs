using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AiEngine.Configurations;

public sealed class AiUsageRecordConfiguration : IEntityTypeConfiguration<AiUsageRecord>
{
    public void Configure(EntityTypeBuilder<AiUsageRecord> builder)
    {
        builder.ToTable("ai_usage_records", "ai_engine");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(e => e.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(v => v.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(e => e.Provider).HasColumnName("provider").HasMaxLength(50).IsRequired();
        builder.Property(e => e.Model).HasColumnName("model").HasMaxLength(100).IsRequired();
        builder.Property(e => e.TaskType).HasColumnName("task_type").HasMaxLength(100).IsRequired();
        builder.Property(e => e.UserId).HasColumnName("user_id");
        builder.Property(e => e.InputTokens).HasColumnName("input_tokens");
        builder.Property(e => e.OutputTokens).HasColumnName("output_tokens");
        builder.Property(e => e.TotalTokens).HasColumnName("total_tokens");
        builder.Property(e => e.EstimatedCostUsd).HasColumnName("estimated_cost_usd").HasPrecision(10, 6);
        builder.Property(e => e.ProcessingTimeMs).HasColumnName("processing_time_ms");
        builder.Property(e => e.Success).HasColumnName("success");
        builder.Property(e => e.ErrorCategory).HasColumnName("error_category").HasMaxLength(100);
        builder.Property(e => e.ErrorDetail).HasColumnName("error_detail").HasMaxLength(2000);
        builder.Property(e => e.RecommendationId).HasColumnName("recommendation_id");
        builder.Property(e => e.ContextDocumentIds).HasColumnName("context_document_ids").HasMaxLength(2000);
        builder.Property(e => e.WasFallback).HasColumnName("was_fallback");
        builder.Property(e => e.FallbackFromProvider).HasColumnName("fallback_from_provider").HasMaxLength(50);

        builder.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(256);
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(256);
        builder.Property(e => e.ModifiedAt).HasColumnName("modified_at");

        builder.HasIndex(e => new { e.TenantId, e.Provider }).HasDatabaseName("ix_ai_usage_records_tenant_provider");
        builder.HasIndex(e => new { e.TenantId, e.TaskType }).HasDatabaseName("ix_ai_usage_records_tenant_task");
        builder.HasIndex(e => new { e.TenantId, e.CreatedAt }).HasDatabaseName("ix_ai_usage_records_tenant_created");
    }
}
