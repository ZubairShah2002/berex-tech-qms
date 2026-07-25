using BerexQms.Domain.ProductCatalog.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.ProductCatalog.Entities;

public sealed class PartRevision : Entity<Guid>, IAuditableEntity
{
    private readonly List<SpecificationParameter> _specificationParameters = [];

    public Guid PartId { get; private set; }
    public string RevisionCode { get; private set; } = string.Empty;
    public RevisionStatus Status { get; private set; }
    public string? Description { get; private set; }
    public string? ChangeReason { get; private set; }
    public DateTime? ReleasedAt { get; private set; }
    public string? ReleasedBy { get; private set; }
    public DateTime? ObsoletedAt { get; private set; }

    public IReadOnlyCollection<SpecificationParameter> SpecificationParameters => _specificationParameters.AsReadOnly();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private PartRevision() { }

    internal static PartRevision Create(
        Guid id,
        TenantId tenantId,
        Guid partId,
        string revisionCode,
        string? description,
        string? changeReason)
    {
        if (string.IsNullOrWhiteSpace(revisionCode))
            throw new DomainException("Revision code is required.");

        return new PartRevision
        {
            Id = id,
            TenantId = tenantId,
            PartId = partId,
            RevisionCode = revisionCode.Trim().ToUpperInvariant(),
            Status = RevisionStatus.Draft,
            Description = description?.Trim(),
            ChangeReason = changeReason?.Trim()
        };
    }

    internal void Release(string releasedBy)
    {
        if (Status != RevisionStatus.Draft)
            throw new DomainException($"Only draft revisions can be released. Current status: {Status}.");

        Status = RevisionStatus.Released;
        ReleasedAt = DateTime.UtcNow;
        ReleasedBy = releasedBy;
    }

    internal void Obsolete()
    {
        if (Status != RevisionStatus.Released)
            throw new DomainException($"Only released revisions can be obsoleted. Current status: {Status}.");

        Status = RevisionStatus.Obsolete;
        ObsoletedAt = DateTime.UtcNow;
    }

    internal void UpdateDraft(string? description, string? changeReason)
    {
        if (Status != RevisionStatus.Draft)
            throw new DomainException("Only draft revisions can be edited.");

        Description = description?.Trim();
        ChangeReason = changeReason?.Trim();
    }

    internal SpecificationParameter AddSpecificationParameter(
        string name,
        ParameterType type,
        string? unit,
        decimal? nominalValue,
        decimal? upperTolerance,
        decimal? lowerTolerance,
        string? textValue,
        bool isCritical)
    {
        if (Status != RevisionStatus.Draft)
            throw new DomainException("Specification parameters can only be added to draft revisions.");

        var sortOrder = _specificationParameters.Count;
        var param = SpecificationParameter.Create(
            Guid.NewGuid(), TenantId, Id,
            name, type, unit, nominalValue, upperTolerance, lowerTolerance,
            textValue, isCritical, sortOrder);

        _specificationParameters.Add(param);
        return param;
    }

    internal void RemoveSpecificationParameter(Guid parameterId)
    {
        if (Status != RevisionStatus.Draft)
            throw new DomainException("Specification parameters can only be removed from draft revisions.");

        var param = _specificationParameters.FirstOrDefault(p => p.Id == parameterId)
                    ?? throw new DomainException("Specification parameter not found.");

        _specificationParameters.Remove(param);
    }
}
