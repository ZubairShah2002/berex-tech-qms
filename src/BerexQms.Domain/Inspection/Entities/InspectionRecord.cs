using BerexQms.Domain.Inspection.Enums;
using BerexQms.Domain.Inspection.Events;
using BerexQms.Domain.Inspection.ValueObjects;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Inspection.Entities;

public sealed class InspectionRecord : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<Measurement> _measurements = [];
    private readonly List<IntegrityGateResult> _gateResults = [];

    public string InspectionNumber { get; private set; } = string.Empty;
    public InspectionType Type { get; private set; }
    public InspectionStatus Status { get; private set; }
    public Guid PartId { get; private set; }
    public Guid? PartRevisionId { get; private set; }
    public string? LotNumber { get; private set; }
    public int? LotSize { get; private set; }
    public int? SampleSize { get; private set; }
    public Guid? SupplierId { get; private set; }
    public Guid? SamplingPlanId { get; private set; }
    public string InspectorId { get; private set; } = string.Empty;
    public InspectionResult? Result { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? CompletedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? RejectedAt { get; private set; }
    public string? RejectedBy { get; private set; }
    public LotDisposition? Disposition { get; private set; }

    public Guid? ChecklistId { get; private set; }
    public InspectionChecklist? Checklist { get; private set; }

    public IReadOnlyCollection<Measurement> Measurements => _measurements.AsReadOnly();
    public IReadOnlyList<IntegrityGateResult> GateResults => _gateResults.AsReadOnly();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private InspectionRecord() { }

    public static InspectionRecord Create(
        Guid id,
        TenantId tenantId,
        string inspectionNumber,
        InspectionType type,
        Guid partId,
        Guid? partRevisionId,
        string? lotNumber,
        int? lotSize,
        int? sampleSize,
        Guid? supplierId,
        Guid? samplingPlanId,
        string inspectorId)
    {
        if (string.IsNullOrWhiteSpace(inspectionNumber))
            throw new DomainException("Inspection number is required.");

        if (string.IsNullOrWhiteSpace(inspectorId))
            throw new DomainException("Inspector ID is required.");

        var record = new InspectionRecord
        {
            Id = id,
            TenantId = tenantId,
            InspectionNumber = inspectionNumber.Trim().ToUpperInvariant(),
            Type = type,
            Status = InspectionStatus.Draft,
            PartId = partId,
            PartRevisionId = partRevisionId,
            LotNumber = lotNumber?.Trim(),
            LotSize = lotSize,
            SampleSize = sampleSize,
            SupplierId = supplierId,
            SamplingPlanId = samplingPlanId,
            InspectorId = inspectorId
        };

        record.AddDomainEvent(new InspectionCreatedEvent(
            id, inspectionNumber, type.ToString(), partId, tenantId.Value));

        return record;
    }

    public void SetChecklist(InspectionChecklist checklist)
    {
        if (Status != InspectionStatus.Draft)
            throw new DomainException("Checklist can only be set on draft inspections.");

        Checklist = checklist;
        ChecklistId = checklist.Id;
    }

    public void AddGateResult(GateType gateType, bool passed, string? detail)
    {
        if (Status != InspectionStatus.Draft)
            throw new DomainException("Gate results can only be added to draft inspections.");

        _gateResults.Add(new IntegrityGateResult(gateType, passed, detail, DateTime.UtcNow));
    }

    public bool AllGatesPassed()
    {
        return _gateResults.Count > 0 && _gateResults.All(g => g.Passed);
    }

    public void StartInspection()
    {
        if (Status != InspectionStatus.Draft)
            throw new DomainException($"Cannot start inspection in status: {Status}.");

        Status = InspectionStatus.InProgress;
    }

    public Measurement AddMeasurement(
        Guid? checklistItemId,
        string characteristicName,
        decimal? measuredValue,
        string? textValue,
        string? unit,
        MeasurementResult result,
        Guid? equipmentId,
        string? operatorId)
    {
        if (Status != InspectionStatus.InProgress)
            throw new DomainException("Measurements can only be recorded on in-progress inspections.");

        var sequenceNumber = _measurements.Count + 1;
        var measurement = Measurement.Create(
            Guid.NewGuid(), TenantId, Id,
            checklistItemId, characteristicName,
            measuredValue, textValue, unit, result,
            equipmentId, operatorId, sequenceNumber);

        _measurements.Add(measurement);
        return measurement;
    }

    public void Complete(string completedBy)
    {
        if (Status != InspectionStatus.InProgress)
            throw new DomainException($"Cannot complete inspection in status: {Status}.");

        if (string.IsNullOrWhiteSpace(completedBy))
            throw new DomainException("Completed by is required.");

        if (_measurements.Count == 0)
            throw new DomainException("At least one measurement must be recorded before completing an inspection.");

        var failCount = _measurements.Count(m => m.Result == MeasurementResult.Fail);
        Result = failCount == 0 ? InspectionResult.Pass : InspectionResult.Fail;

        Status = InspectionStatus.PendingApproval;
        CompletedAt = DateTime.UtcNow;
        CompletedBy = completedBy;

        AddDomainEvent(new InspectionCompletedEvent(
            Id, InspectionNumber, Type.ToString(), Result.Value.ToString(),
            PartId, _measurements.Count, failCount, TenantId.Value));
    }

    public void Approve(string approvedBy)
    {
        if (Status != InspectionStatus.PendingApproval)
            throw new DomainException($"Cannot approve inspection in status: {Status}.");

        if (string.IsNullOrWhiteSpace(approvedBy))
            throw new DomainException("Approved by is required.");

        Status = InspectionStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approvedBy;
    }

    public void Reject(string rejectedBy, string? notes)
    {
        if (Status != InspectionStatus.PendingApproval)
            throw new DomainException($"Cannot reject inspection in status: {Status}.");

        if (string.IsNullOrWhiteSpace(rejectedBy))
            throw new DomainException("Rejected by is required.");

        Status = InspectionStatus.Rejected;
        RejectedAt = DateTime.UtcNow;
        RejectedBy = rejectedBy;
        if (notes != null) Notes = notes.Trim();
    }

    public void SetDisposition(DispositionType type, string justification, string approvedBy)
    {
        if (Status != InspectionStatus.Approved)
            throw new DomainException("Disposition can only be set on approved inspections.");

        if (Result != InspectionResult.Fail)
            throw new DomainException("Disposition is only required for failed inspections.");

        if (string.IsNullOrWhiteSpace(justification))
            throw new DomainException("Disposition justification is required.");

        Disposition = new LotDisposition(type, justification.Trim(), approvedBy, DateTime.UtcNow);
    }

    public void Cancel()
    {
        if (Status is InspectionStatus.Approved or InspectionStatus.Cancelled)
            throw new DomainException($"Cannot cancel inspection in status: {Status}.");

        Status = InspectionStatus.Cancelled;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
    }
}
