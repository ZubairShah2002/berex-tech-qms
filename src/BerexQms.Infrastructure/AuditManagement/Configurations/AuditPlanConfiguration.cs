using BerexQms.Domain.AuditManagement.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AuditManagement.Configurations;

public sealed class AuditPlanConfiguration : IEntityTypeConfiguration<AuditPlan>
{
    public void Configure(EntityTypeBuilder<AuditPlan> builder)
    {
        builder.ToTable("audit_plans", "audit");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(a => a.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.Property(a => a.PlanName)
            .HasColumnName("plan_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Year)
            .HasColumnName("year")
            .IsRequired();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(a => a.Scope)
            .HasColumnName("scope")
            .HasMaxLength(2000);

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(a => a.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.ModifiedBy)
            .HasColumnName("modified_by")
            .HasMaxLength(200);

        builder.Property(a => a.ModifiedAt)
            .HasColumnName("modified_at");

        builder.HasMany(a => a.Audits)
            .WithOne()
            .HasForeignKey(r => r.AuditPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.TenantId).HasDatabaseName("ix_audit_plans_tenant_id");
        builder.HasIndex(a => new { a.TenantId, a.PlanName, a.Year })
            .IsUnique()
            .HasDatabaseName("ix_audit_plans_tenant_name_year");
        builder.HasIndex(a => a.Year).HasDatabaseName("ix_audit_plans_year");
        builder.HasIndex(a => a.CreatedAt).HasDatabaseName("ix_audit_plans_created_at");

        builder.Ignore(a => a.DomainEvents);
    }
}
