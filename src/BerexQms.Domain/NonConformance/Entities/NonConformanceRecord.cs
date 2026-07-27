using BerexQms.Domain.NonConformance.Enums;
using BerexQms.Domain.NonConformance.Events;
using BerexQms.Domain.NonConformance.ValueObjects;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.NonConformance.Entities;

public sealed class NonConformanceRecord : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<ContainmentAction> _containmentActions = [];
    private readonly List<Investigation> _investigations = [];

    public string NcrNumber { get; private set; } = string.Empty;
    public NCStatus Status { get; private set; }
    public NCSeverity Severity { get; private set; }
    public NCSource Source { get; private set; }
    public DetectionPoint DetectionPoint { get; private set; }
    public string Description { get; private set; } = string.Empty;

    public Guid PartId { get; private set; }
    public Guid? PartRevisionId { get; private set; }
    public string? LotNumber { get; private set; }
    public string? SerialNumber { get; private set; }
    public Guid? SupplierId { get; private set; }
    public string? SupplierLotNumber { get; private set; }
    public string? WorkOrderNumber { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Guid? SourceInspectionId { get; private set; }

    public int QuantityAffected { get; private set; }
    public int QuantityDefective { get; private set; }

    public NCClassification? Classification { get; private set; }
    public DispositionRecord? Disposition { get; private set; }
    public ImpactAssessment? ImpactAssessment { get; private set; }

    public string? AssignedTo { get; private set; }
    public Guid? CapaId { get; private set; }

    public DateTime? ClosedAt { get; private set; }
    public string? ClosedBy { get; private set; }
    public DateTime? ReopenedAt { get; private set; }
    public string? ReopenedBy { get; private set; }
    public string? ReopenReason { get; private set; }
    public string? ClosureNotes { get; private set; }

    public IReadOnlyCollection<ContainmentAction> ContainmentActions => _containmentActions.AsReadOnly();
    public IReadOnlyCollection<Investigation> Investigations => _investigations.AsReadOnly();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private NonConformanceRecord() { }

    public static NonConformanceRecord Create(
        Guid id,
        TenantId tenantId,
        string ncrNumber,
        NCSeverity severity,
        NCSource source,
        DetectionPoint detectionPoint,
        string description,
        Guid partId,
        Guid? partRevisionId,
        string? lotNumber,
        string? serialNumber,
        Guid? supplierId,
        string? supplierLotNumber,
        string? workOrderNumber,
        Guid? customerId,
        Guid? sourceInspectionId,
        int quantityAffected,
        int quantityDefective)
    {
        if (string.IsNullOrWhiteSpace(ncrNumber))
            throw new DomainException("NCR number is required.");

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");

        if (quantityAffected < 0)
            throw new DomainException("Quantity affected cannot be negative.");

        if (quantityDefective < 0)
            throw new DomainException("Quantity defective cannot be negative.");

        if (quantityDefective > quantityAffected)
            throw new DomainException("Quantity defective cannot exceed quantity affected.");

        var record = new NonConformanceRecord
        {
            Id = id,
            TenantId = tenantId,
            NcrNumber = ncrNumber.Trim().ToUpperInvariant(),
            Status = NCStatus.Open,
            Severity = severity,
            Source = source,
            DetectionPoint = detectionPoint,
            Description = description.Trim(),
            PartId = partId,
            PartRevisionId = partRevisionId,
            LotNumber = lotNumber?.Trim(),
            SerialNumber = serialNumber?.Trim(),
            SupplierId = supplierId,
            SupplierLotNumber = supplierLotNumber?.Trim(),
            WorkOrderNumber = workOrderNumber?.Trim(),
            CustomerId = customerId,
            SourceInspectionId = sourceInspectionId,
            QuantityAffected = quantityAffected,
            QuantityDefective = quantityDefective
        };

        record.AddDomainEvent(new NonConformanceRaisedEvent(
            id, ncrNumber, severity.ToString(), partId,
            null, tenantId.Value));

        return record;
    }

    public void SetClassification(string category, string defectType, string? defectCode)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new DomainException("Classification category is required.");

        if (string.IsNullOrWhiteSpace(defectType))
            throw new DomainException("Defect type is required.");

        Classification = new NCClassification(category.Trim(), defectType.Trim(), defectCode?.Trim());
    }

    public void SetImpactAssessment(int affectedQuantity, bool shippedProductAffected, string? customerImpact)
    {
        if (affectedQuantity < 0)
            throw new DomainException("Affected quantity cannot be negative.");

        ImpactAssessment = new ImpactAssessment(affectedQuantity, shippedProductAffected, customerImpact?.Trim());
    }

    public void AssignInvestigator(string investigatorId)
    {
        if (Status is not (NCStatus.Open or NCStatus.Reopened))
            throw new DomainException($"Cannot assign investigator in status: {Status}.");

        if (string.IsNullOrWhiteSpace(investigatorId))
            throw new DomainException("Investigator ID is required.");

        if (Severity == NCSeverity.Critical && !HasVerifiedContainment())
            throw new DomainException("Critical NCs require verified containment before investigation can proceed.");

        AssignedTo = investigatorId;
        Status = NCStatus.UnderInvestigation;

        var investigation = Investigation.Create(
            Guid.NewGuid(), TenantId, Id, investigatorId);
        _investigations.Add(investigation);
    }

    public ContainmentAction AddContainmentAction(string description, string actionTakenBy)
    {
        if (Status is NCStatus.Closed)
            throw new DomainException("Cannot add containment actions to a closed NC.");

        var action = ContainmentAction.Create(
            Guid.NewGuid(), TenantId, Id, description, actionTakenBy);
        _containmentActions.Add(action);
        return action;
    }

    public void VerifyContainment(Guid containmentActionId, string verifiedBy)
    {
        var action = _containmentActions.FirstOrDefault(a => a.Id == containmentActionId)
            ?? throw new DomainException("Containment action not found.");

        action.Verify(verifiedBy);
    }

    public bool HasVerifiedContainment()
    {
        return _containmentActions.Count > 0 && _containmentActions.Any(a => a.IsVerified);
    }

    public void SubmitInvestigation(string? methodology, string rootCause, string findings)
    {
        if (Status != NCStatus.UnderInvestigation)
            throw new DomainException($"Cannot submit investigation in status: {Status}.");

        var activeInvestigation = _investigations
            .OrderByDescending(i => i.StartedAt)
            .FirstOrDefault(i => i.CompletedAt is null)
            ?? throw new DomainException("No active investigation found.");

        activeInvestigation.SubmitFindings(methodology, rootCause, findings);
        Status = NCStatus.PendingDisposition;
    }

    public void RecordDisposition(NCDispositionType type, string justification, string approvedBy)
    {
        if (Status != NCStatus.PendingDisposition)
            throw new DomainException($"Cannot record disposition in status: {Status}.");

        if (string.IsNullOrWhiteSpace(justification))
            throw new DomainException("Disposition justification is required.");

        if (string.IsNullOrWhiteSpace(approvedBy))
            throw new DomainException("Disposition approver is required.");

        if ((Severity == NCSeverity.Critical || Severity == NCSeverity.Major) && CapaId is null)
            throw new DomainException("Critical and Major non-conformances require a linked CAPA before closure.");

        Disposition = new DispositionRecord(type, justification.Trim(), approvedBy, DateTime.UtcNow);
        Status = NCStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        ClosedBy = approvedBy;

        AddDomainEvent(new NonConformanceClosedEvent(
            Id, NcrNumber, type.ToString(), TenantId.Value));
    }

    public void RequestMoreInfo(string reason)
    {
        if (Status != NCStatus.PendingDisposition)
            throw new DomainException($"Cannot request more info in status: {Status}.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Reason for requesting more information is required.");

        Status = NCStatus.UnderInvestigation;

        var investigation = Investigation.Create(
            Guid.NewGuid(), TenantId, Id,
            AssignedTo ?? throw new DomainException("No investigator assigned."));
        _investigations.Add(investigation);
    }

    public void CloseAsDuplicate(string closedBy, string notes)
    {
        if (Status != NCStatus.Open)
            throw new DomainException("Only open NCs can be closed as duplicate.");

        if (string.IsNullOrWhiteSpace(closedBy))
            throw new DomainException("Closed by is required.");

        if (string.IsNullOrWhiteSpace(notes))
            throw new DomainException("Duplicate/invalid closure requires notes.");

        Status = NCStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        ClosedBy = closedBy;
        ClosureNotes = notes.Trim();

        AddDomainEvent(new NonConformanceClosedEvent(
            Id, NcrNumber, "Duplicate", TenantId.Value));
    }

    public void Reopen(string reopenedBy, string reason)
    {
        if (Status != NCStatus.Closed)
            throw new DomainException($"Cannot reopen NC in status: {Status}.");

        if (string.IsNullOrWhiteSpace(reopenedBy))
            throw new DomainException("Reopened by is required.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Reopen reason is required.");

        Status = NCStatus.Reopened;
        ReopenedAt = DateTime.UtcNow;
        ReopenedBy = reopenedBy;
        ReopenReason = reason.Trim();
        Disposition = null;
        CapaId = null;
        ClosedAt = null;
        ClosedBy = null;
    }

    public void LinkCapa(Guid capaId)
    {
        if (capaId == Guid.Empty)
            throw new DomainException("CAPA ID is required.");

        CapaId = capaId;
    }
}
