using BerexQms.Domain.Identity.Entities;
using BerexQms.Domain.Identity.Enums;
using BerexQms.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Identity.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "identity");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(u => u.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion(t => t.Value, v => TenantId.From(v))
            .IsRequired();

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("email")
                .HasMaxLength(254)
                .IsRequired();

            email.HasIndex(e => e.Value).IsUnique();
        });

        builder.OwnsOne(u => u.Name, name =>
        {
            name.Property(n => n.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .IsRequired();

            name.Property(n => n.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .IsRequired();

            name.Property(n => n.DisplayName)
                .HasColumnName("display_name")
                .HasMaxLength(201)
                .IsRequired();
        });

        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);

        builder.Property(u => u.Department)
            .HasColumnName("department")
            .HasMaxLength(100);

        builder.Property(u => u.JobTitle)
            .HasColumnName("job_title")
            .HasMaxLength(100);

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        builder.Property(u => u.FailedLoginAttempts)
            .HasColumnName("failed_login_attempts")
            .HasDefaultValue(0);

        builder.Property(u => u.LockoutEndUtc)
            .HasColumnName("lockout_end_utc");

        builder.Property(u => u.RefreshToken)
            .HasColumnName("refresh_token")
            .HasMaxLength(256);

        builder.Property(u => u.RefreshTokenExpiryUtc)
            .HasColumnName("refresh_token_expiry_utc");

        builder.Property(u => u.CreatedBy)
            .HasColumnName("created_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.ModifiedBy)
            .HasColumnName("modified_by")
            .HasMaxLength(100);

        builder.Property(u => u.ModifiedAt)
            .HasColumnName("modified_at");

        builder.HasMany(u => u.UserRoles)
            .WithOne()
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(u => u.TenantId).HasDatabaseName("ix_users_tenant_id");
        builder.HasIndex(u => u.RefreshToken).HasDatabaseName("ix_users_refresh_token")
            .HasFilter("refresh_token IS NOT NULL");

        builder.Navigation(u => u.UserRoles).AutoInclude();

        builder.Ignore(u => u.DomainEvents);
    }
}
