using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("audit_log", "shared");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.Timestamp).HasColumnName("timestamp").IsRequired();
        builder.Property(e => e.EntityType).HasColumnName("entity_type").HasMaxLength(256).IsRequired();
        builder.Property(e => e.EntityId).HasColumnName("entity_id").IsRequired();
        builder.Property(e => e.Action).HasColumnName("action").HasMaxLength(64).IsRequired();
        builder.Property(e => e.OldValue).HasColumnName("old_value").HasColumnType("jsonb");
        builder.Property(e => e.NewValue).HasColumnName("new_value").HasColumnType("jsonb");
        builder.Property(e => e.SourceIp).HasColumnName("source_ip").HasMaxLength(45);
        builder.Property(e => e.CorrelationId).HasColumnName("correlation_id").HasMaxLength(64);
        builder.Property(e => e.ModuleName).HasColumnName("module_name").HasMaxLength(128).IsRequired();

        builder.HasIndex(e => new { e.TenantId, e.EntityType, e.EntityId })
            .HasDatabaseName("ix_audit_log_tenant_entity");
        builder.HasIndex(e => e.Timestamp)
            .IsDescending()
            .HasDatabaseName("ix_audit_log_timestamp");
        builder.HasIndex(e => e.CorrelationId)
            .HasDatabaseName("ix_audit_log_correlation");
    }
}

public class DomainEventOutboxConfiguration : IEntityTypeConfiguration<DomainEventOutboxEntry>
{
    public void Configure(EntityTypeBuilder<DomainEventOutboxEntry> builder)
    {
        builder.ToTable("domain_events_outbox", "shared");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.EventId).HasColumnName("event_id").IsRequired();
        builder.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(512).IsRequired();
        builder.Property(e => e.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(e => e.AggregateType).HasColumnName("aggregate_type").HasMaxLength(256).IsRequired();
        builder.Property(e => e.AggregateId).HasColumnName("aggregate_id").IsRequired();
        builder.Property(e => e.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(e => e.OccurredOn).HasColumnName("occurred_on").IsRequired();
        builder.Property(e => e.ProcessedOn).HasColumnName("processed_on");
        builder.Property(e => e.Error).HasColumnName("error");
        builder.Property(e => e.RetryCount).HasColumnName("retry_count").HasDefaultValue(0);

        builder.HasIndex(e => e.EventId).IsUnique().HasDatabaseName("ix_outbox_event_id");
        builder.HasIndex(e => e.OccurredOn)
            .HasFilter("processed_on IS NULL")
            .HasDatabaseName("ix_outbox_unprocessed");
    }
}
