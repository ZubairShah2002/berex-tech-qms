using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Training.Entities;

public sealed class Qualification : AggregateRoot<Guid>, IAuditableEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ScopeProductFamily { get; private set; }
    public string? ScopeInspectionType { get; private set; }
    public string? ScopeProcessArea { get; private set; }
    public int ValidityMonths { get; private set; }
    public int RenewalWindowDays { get; private set; }
    public bool IsActive { get; private set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private Qualification() { }

    public static Qualification Create(
        Guid id,
        TenantId tenantId,
        string code,
        string name,
        string? description,
        string? scopeProductFamily,
        string? scopeInspectionType,
        string? scopeProcessArea,
        int validityMonths,
        int renewalWindowDays)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Qualification code is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Qualification name is required.");
        if (validityMonths <= 0)
            throw new DomainException("Validity period must be greater than zero.");
        if (renewalWindowDays < 0)
            throw new DomainException("Renewal window cannot be negative.");

        return new Qualification
        {
            Id = id,
            TenantId = tenantId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            ScopeProductFamily = scopeProductFamily?.Trim(),
            ScopeInspectionType = scopeInspectionType?.Trim(),
            ScopeProcessArea = scopeProcessArea?.Trim(),
            ValidityMonths = validityMonths,
            RenewalWindowDays = renewalWindowDays,
            IsActive = true,
        };
    }

    public void Update(
        string name,
        string? description,
        string? scopeProductFamily,
        string? scopeInspectionType,
        string? scopeProcessArea,
        int validityMonths,
        int renewalWindowDays)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Qualification name is required.");
        if (validityMonths <= 0)
            throw new DomainException("Validity period must be greater than zero.");
        if (renewalWindowDays < 0)
            throw new DomainException("Renewal window cannot be negative.");

        Name = name.Trim();
        Description = description?.Trim();
        ScopeProductFamily = scopeProductFamily?.Trim();
        ScopeInspectionType = scopeInspectionType?.Trim();
        ScopeProcessArea = scopeProcessArea?.Trim();
        ValidityMonths = validityMonths;
        RenewalWindowDays = renewalWindowDays;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
