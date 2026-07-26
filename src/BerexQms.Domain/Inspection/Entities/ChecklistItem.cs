using BerexQms.Domain.Inspection.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Inspection.Entities;

public sealed class ChecklistItem : Entity<Guid>
{
    public Guid ChecklistId { get; private set; }
    public string CharacteristicName { get; private set; } = string.Empty;
    public string? SpecificationLimit { get; private set; }
    public decimal? NominalValue { get; private set; }
    public decimal? UpperLimit { get; private set; }
    public decimal? LowerLimit { get; private set; }
    public string? Unit { get; private set; }
    public bool IsCritical { get; private set; }
    public int SortOrder { get; private set; }

    private ChecklistItem() { }

    internal static ChecklistItem Create(
        Guid id,
        TenantId tenantId,
        Guid checklistId,
        string characteristicName,
        string? specificationLimit,
        decimal? nominalValue,
        decimal? upperLimit,
        decimal? lowerLimit,
        string? unit,
        bool isCritical,
        int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(characteristicName))
            throw new DomainException("Characteristic name is required.");

        return new ChecklistItem
        {
            Id = id,
            TenantId = tenantId,
            ChecklistId = checklistId,
            CharacteristicName = characteristicName.Trim(),
            SpecificationLimit = specificationLimit?.Trim(),
            NominalValue = nominalValue,
            UpperLimit = upperLimit,
            LowerLimit = lowerLimit,
            Unit = unit?.Trim(),
            IsCritical = isCritical,
            SortOrder = sortOrder
        };
    }
}
