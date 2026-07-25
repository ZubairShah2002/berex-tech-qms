using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Inspection.Entities;

public sealed class InspectionChecklist : Entity<Guid>
{
    private readonly List<ChecklistItem> _items = [];

    public Guid InspectionId { get; private set; }
    public Guid PartRevisionId { get; private set; }
    public string RevisionCode { get; private set; } = string.Empty;
    public DateTime SnapshotAt { get; private set; }

    public IReadOnlyCollection<ChecklistItem> Items => _items.AsReadOnly();

    private InspectionChecklist() { }

    internal static InspectionChecklist Create(
        Guid id,
        TenantId tenantId,
        Guid inspectionId,
        Guid partRevisionId,
        string revisionCode)
    {
        if (string.IsNullOrWhiteSpace(revisionCode))
            throw new DomainException("Revision code is required.");

        return new InspectionChecklist
        {
            Id = id,
            TenantId = tenantId,
            InspectionId = inspectionId,
            PartRevisionId = partRevisionId,
            RevisionCode = revisionCode.Trim(),
            SnapshotAt = DateTime.UtcNow
        };
    }

    internal ChecklistItem AddItem(
        string characteristicName,
        string? specificationLimit,
        decimal? nominalValue,
        decimal? upperLimit,
        decimal? lowerLimit,
        string? unit,
        bool isCritical)
    {
        var sortOrder = _items.Count;
        var item = ChecklistItem.Create(
            Guid.NewGuid(), TenantId, Id,
            characteristicName, specificationLimit,
            nominalValue, upperLimit, lowerLimit,
            unit, isCritical, sortOrder);

        _items.Add(item);
        return item;
    }
}
