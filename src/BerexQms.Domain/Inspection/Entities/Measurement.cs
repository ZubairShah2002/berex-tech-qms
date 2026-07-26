using BerexQms.Domain.Inspection.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Inspection.Entities;

public sealed class Measurement : Entity<Guid>
{
    public Guid InspectionId { get; private set; }
    public Guid? ChecklistItemId { get; private set; }
    public string CharacteristicName { get; private set; } = string.Empty;
    public decimal? MeasuredValue { get; private set; }
    public string? TextValue { get; private set; }
    public string? Unit { get; private set; }
    public MeasurementResult Result { get; private set; }
    public Guid? EquipmentId { get; private set; }
    public string? OperatorId { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public int SequenceNumber { get; private set; }

    private Measurement() { }

    internal static Measurement Create(
        Guid id,
        TenantId tenantId,
        Guid inspectionId,
        Guid? checklistItemId,
        string characteristicName,
        decimal? measuredValue,
        string? textValue,
        string? unit,
        MeasurementResult result,
        Guid? equipmentId,
        string? operatorId,
        int sequenceNumber)
    {
        if (string.IsNullOrWhiteSpace(characteristicName))
            throw new DomainException("Characteristic name is required.");

        return new Measurement
        {
            Id = id,
            TenantId = tenantId,
            InspectionId = inspectionId,
            ChecklistItemId = checklistItemId,
            CharacteristicName = characteristicName.Trim(),
            MeasuredValue = measuredValue,
            TextValue = textValue?.Trim(),
            Unit = unit?.Trim(),
            Result = result,
            EquipmentId = equipmentId,
            OperatorId = operatorId,
            RecordedAt = DateTime.UtcNow,
            SequenceNumber = sequenceNumber
        };
    }
}
