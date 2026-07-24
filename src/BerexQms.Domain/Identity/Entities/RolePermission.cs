namespace BerexQms.Domain.Identity.Entities;

public sealed class RolePermission
{
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }
    public Permission Permission { get; private set; } = null!;

    private RolePermission() { }

    internal static RolePermission Create(Guid roleId, Guid permissionId)
    {
        return new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
    }
}
