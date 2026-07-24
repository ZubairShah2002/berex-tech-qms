using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Identity.Entities;

public sealed class Role : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<RolePermission> _rolePermissions = [];

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }
    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private Role() { }

    public static Role Create(Guid id, TenantId tenantId, string name, string? description = null, bool isSystemRole = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name is required.");

        if (name.Length > 100)
            throw new DomainException("Role name cannot exceed 100 characters.");

        return new Role
        {
            Id = id,
            TenantId = tenantId,
            Name = name.Trim(),
            Description = description?.Trim(),
            IsSystemRole = isSystemRole
        };
    }

    public void UpdateDetails(string name, string? description)
    {
        if (IsSystemRole)
            throw new DomainException("System roles cannot be modified.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Role name is required.");

        Name = name.Trim();
        Description = description?.Trim();
    }

    public void AddPermission(Guid permissionId)
    {
        if (_rolePermissions.Any(rp => rp.PermissionId == permissionId))
            return;

        _rolePermissions.Add(RolePermission.Create(Id, permissionId));
    }

    public void RemovePermission(Guid permissionId)
    {
        var existing = _rolePermissions.FirstOrDefault(rp => rp.PermissionId == permissionId);
        if (existing is not null)
            _rolePermissions.Remove(existing);
    }

    public bool HasPermission(Guid permissionId) =>
        _rolePermissions.Any(rp => rp.PermissionId == permissionId);
}
