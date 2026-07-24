using BerexQms.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BerexQms.Infrastructure.Identity.Configurations;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles", "identity");

        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.Property(ur => ur.UserId)
            .HasColumnName("user_id");

        builder.Property(ur => ur.RoleId)
            .HasColumnName("role_id");

        builder.Property(ur => ur.AssignedAt)
            .HasColumnName("assigned_at")
            .IsRequired();

        builder.Property(ur => ur.AssignedBy)
            .HasColumnName("assigned_by")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(ur => ur.Role).AutoInclude();
    }
}
