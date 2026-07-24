namespace BerexQms.Domain.Identity.Entities;

public sealed class UserRole
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public string AssignedBy { get; private set; } = string.Empty;
    public Role Role { get; private set; } = null!;

    private UserRole() { }

    internal static UserRole Create(Guid userId, Guid roleId, string assignedBy)
    {
        return new UserRole
        {
            UserId = userId,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = assignedBy
        };
    }
}
