using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Identity.Entities;

public sealed class Permission : Entity<Guid>
{
    public string Module { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public string FullName => $"{Module}.{Action}";

    private Permission() { }

    public static Permission Create(Guid id, TenantId tenantId, string module, string action, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(module))
            throw new ArgumentException("Module is required.", nameof(module));

        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action is required.", nameof(action));

        return new Permission
        {
            Id = id,
            TenantId = tenantId,
            Module = module.Trim(),
            Action = action.Trim(),
            Description = description?.Trim()
        };
    }
}
