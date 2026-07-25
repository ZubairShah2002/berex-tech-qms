using BerexQms.Domain.ProductCatalog.Enums;
using BerexQms.Domain.ProductCatalog.Events;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.ProductCatalog.Entities;

public sealed class Part : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<PartRevision> _revisions = [];
    private readonly List<BomReference> _bomReferences = [];

    public string PartNumber { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ProductFamily { get; private set; }
    public string? Category { get; private set; }
    public SerializationMode SerializationMode { get; private set; }
    public PartStatus Status { get; private set; }
    public string? UnitOfMeasure { get; private set; }

    public IReadOnlyCollection<PartRevision> Revisions => _revisions.AsReadOnly();
    public IReadOnlyCollection<BomReference> BomReferences => _bomReferences.AsReadOnly();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private Part() { }

    public static Part Create(
        Guid id,
        TenantId tenantId,
        string partNumber,
        string name,
        string? description,
        string? productFamily,
        string? category,
        SerializationMode serializationMode,
        string? unitOfMeasure)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
            throw new DomainException("Part number is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Part name is required.");

        var part = new Part
        {
            Id = id,
            TenantId = tenantId,
            PartNumber = partNumber.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            ProductFamily = productFamily?.Trim(),
            Category = category?.Trim(),
            SerializationMode = serializationMode,
            Status = PartStatus.Active,
            UnitOfMeasure = unitOfMeasure?.Trim()
        };

        part.AddDomainEvent(new PartCreatedEvent(id, part.PartNumber, name, tenantId.Value));

        return part;
    }

    public void UpdateDetails(
        string name,
        string? description,
        string? productFamily,
        string? category,
        SerializationMode serializationMode,
        string? unitOfMeasure)
    {
        if (Status == PartStatus.Obsolete)
            throw new DomainException("Cannot update an obsolete part.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Part name is required.");

        Name = name.Trim();
        Description = description?.Trim();
        ProductFamily = productFamily?.Trim();
        Category = category?.Trim();
        SerializationMode = serializationMode;
        UnitOfMeasure = unitOfMeasure?.Trim();
    }

    public void Activate()
    {
        if (Status == PartStatus.Active)
            throw new DomainException("Part is already active.");

        if (Status == PartStatus.Obsolete)
            throw new DomainException("Cannot activate an obsolete part.");

        Status = PartStatus.Active;
    }

    public void Deactivate()
    {
        if (Status == PartStatus.Inactive)
            throw new DomainException("Part is already inactive.");

        if (Status == PartStatus.Obsolete)
            throw new DomainException("Cannot deactivate an obsolete part.");

        Status = PartStatus.Inactive;
    }

    public void Obsolete()
    {
        if (Status == PartStatus.Obsolete)
            throw new DomainException("Part is already obsolete.");

        foreach (var revision in _revisions.Where(r => r.Status == RevisionStatus.Released))
        {
            revision.Obsolete();
        }

        Status = PartStatus.Obsolete;
        AddDomainEvent(new PartObsoletedEvent(Id, PartNumber, TenantId.Value));
    }

    public PartRevision CreateRevision(string revisionCode, string? description, string? changeReason)
    {
        if (Status == PartStatus.Obsolete)
            throw new DomainException("Cannot create revisions for an obsolete part.");

        if (_revisions.Any(r =>
                r.RevisionCode.Equals(revisionCode.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Revision '{revisionCode}' already exists for this part.");

        var revision = PartRevision.Create(
            Guid.NewGuid(), TenantId, Id,
            revisionCode, description, changeReason);

        _revisions.Add(revision);
        return revision;
    }

    public void ReleaseRevision(Guid revisionId, string releasedBy)
    {
        var revision = _revisions.FirstOrDefault(r => r.Id == revisionId)
                       ?? throw new DomainException("Revision not found.");

        foreach (var released in _revisions.Where(r => r.Status == RevisionStatus.Released))
        {
            released.Obsolete();
        }

        revision.Release(releasedBy);

        AddDomainEvent(new PartRevisionReleasedEvent(
            Id, revisionId, revision.RevisionCode, PartNumber, TenantId.Value));
    }

    public PartRevision? GetCurrentRevision()
    {
        return _revisions.FirstOrDefault(r => r.Status == RevisionStatus.Released);
    }

    public SpecificationParameter AddSpecificationParameter(
        Guid revisionId,
        string name,
        ParameterType type,
        string? unit,
        decimal? nominalValue,
        decimal? upperTolerance,
        decimal? lowerTolerance,
        string? textValue,
        bool isCritical)
    {
        var revision = _revisions.FirstOrDefault(r => r.Id == revisionId)
                       ?? throw new DomainException("Revision not found.");

        return revision.AddSpecificationParameter(
            name, type, unit, nominalValue, upperTolerance, lowerTolerance,
            textValue, isCritical);
    }

    public BomReference AddBomReference(Guid childPartId, decimal quantity, string? referenceDesignator)
    {
        if (Status == PartStatus.Obsolete)
            throw new DomainException("Cannot modify BOM for an obsolete part.");

        if (_bomReferences.Any(b => b.ChildPartId == childPartId))
            throw new DomainException("This child part already exists in the BOM.");

        var sortOrder = _bomReferences.Count;
        var bomRef = BomReference.Create(
            Guid.NewGuid(), TenantId, Id, childPartId,
            quantity, referenceDesignator, sortOrder);

        _bomReferences.Add(bomRef);
        return bomRef;
    }

    public void RemoveBomReference(Guid bomReferenceId)
    {
        if (Status == PartStatus.Obsolete)
            throw new DomainException("Cannot modify BOM for an obsolete part.");

        var bomRef = _bomReferences.FirstOrDefault(b => b.Id == bomReferenceId)
                     ?? throw new DomainException("BOM reference not found.");

        _bomReferences.Remove(bomRef);
    }
}
