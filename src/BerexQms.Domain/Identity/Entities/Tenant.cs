using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Identity.Entities;

public sealed class Tenant : AggregateRoot<Guid>, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? TimeZone { get; private set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private Tenant() { }

    public static Tenant Create(Guid id, string name, string code, string? contactEmail = null, string? timeZone = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tenant name is required.");

        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Tenant code is required.");

        if (code.Length > 20)
            throw new DomainException("Tenant code cannot exceed 20 characters.");

        return new Tenant
        {
            Id = id,
            TenantId = TenantId.From(id),
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            IsActive = true,
            ContactEmail = contactEmail?.Trim(),
            TimeZone = timeZone ?? "UTC"
        };
    }

    public void UpdateDetails(string name, string? contactEmail, string? timeZone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tenant name is required.");

        Name = name.Trim();
        ContactEmail = contactEmail?.Trim();
        TimeZone = timeZone ?? TimeZone;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
