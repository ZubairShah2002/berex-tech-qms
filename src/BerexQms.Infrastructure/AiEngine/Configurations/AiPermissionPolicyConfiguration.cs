using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.AiEngine.Configurations;

public sealed class AiPermissionPolicyConfiguration : IEntityTypeConfiguration<AiPermissionPolicy>
{
    public void Configure(EntityTypeBuilder<AiPermissionPolicy> builder)
    {
        builder.ToTable("ai_permission_policies", "ai_engine");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TenantId)
            .HasConversion(v => v.Value, v => TenantId.From(v))
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.PermissionLevel).HasColumnName("permission_level").HasMaxLength(50).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(x => x.GrantedByUserId).HasColumnName("granted_by_user_id").HasMaxLength(100);
        builder.Property(x => x.GrantedAt).HasColumnName("granted_at").IsRequired();
        builder.Property(x => x.RevokedByUserId).HasColumnName("revoked_by_user_id").HasMaxLength(100);
        builder.Property(x => x.RevokedAt).HasColumnName("revoked_at");
        builder.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(500);

        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(100).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
        builder.Property(x => x.ModifiedAt).HasColumnName("modified_at");

        builder.Ignore(x => x.DomainEvents);

        // Only one active policy per user per tenant
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.IsActive })
            .HasFilter("is_active = true")
            .IsUnique()
            .HasDatabaseName("ix_ai_permission_policies_tenant_user_active");

        builder.HasIndex(x => new { x.TenantId, x.UserId })
            .HasDatabaseName("ix_ai_permission_policies_tenant_user");
    }
}
