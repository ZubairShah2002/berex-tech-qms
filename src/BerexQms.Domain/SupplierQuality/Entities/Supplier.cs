using BerexQms.Domain.SupplierQuality.Enums;
using BerexQms.Domain.SupplierQuality.Events;
using BerexQms.Domain.SupplierQuality.ValueObjects;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.SupplierQuality.Entities;

public sealed class Supplier : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<SupplierApproval> _approvals = [];
    private readonly List<SupplierScorecard> _scorecards = [];
    private readonly List<SCARRecord> _scars = [];
    private readonly List<ApprovedPart> _approvedParts = [];

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string RiskLevel { get; private set; } = string.Empty;
    public string? Tier { get; private set; }
    public DateTime? ApprovedSince { get; private set; }
    public SupplierContact? PrimaryContact { get; private set; }
    public SupplierRiskAssessment? RiskAssessment { get; private set; }

    public IReadOnlyCollection<SupplierApproval> Approvals => _approvals.AsReadOnly();
    public IReadOnlyCollection<SupplierScorecard> Scorecards => _scorecards.AsReadOnly();
    public IReadOnlyCollection<SCARRecord> Scars => _scars.AsReadOnly();
    public IReadOnlyCollection<ApprovedPart> ApprovedParts => _approvedParts.AsReadOnly();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private Supplier() { }

    public static Supplier Create(
        Guid id,
        TenantId tenantId,
        string code,
        string name,
        string? tier,
        string? contactName,
        string? contactRole,
        string? contactEmail,
        string? contactPhone)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Supplier code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Supplier name is required.");

        var supplier = new Supplier
        {
            Id = id,
            TenantId = tenantId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Status = SupplierStatus.Prospective.ToString(),
            RiskLevel = Enums.RiskLevel.Low.ToString(),
            Tier = tier?.Trim(),
        };

        if (!string.IsNullOrWhiteSpace(contactName) && !string.IsNullOrWhiteSpace(contactEmail))
        {
            supplier.PrimaryContact = new SupplierContact(
                contactName.Trim(), contactRole?.Trim() ?? string.Empty, contactEmail.Trim(), contactPhone?.Trim());
        }

        return supplier;
    }

    public void UpdateDetails(string name, string? tier, string? contactName, string? contactRole, string? contactEmail, string? contactPhone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Supplier name is required.");

        Name = name.Trim();
        Tier = tier?.Trim();

        if (!string.IsNullOrWhiteSpace(contactName) && !string.IsNullOrWhiteSpace(contactEmail))
        {
            PrimaryContact = new SupplierContact(
                contactName.Trim(), contactRole?.Trim() ?? string.Empty, contactEmail.Trim(), contactPhone?.Trim());
        }
    }

    public void Approve(DateTime approvedDate)
    {
        Status = SupplierStatus.Approved.ToString();
        ApprovedSince = approvedDate;
    }

    public void SetConditionalApproval()
    {
        Status = SupplierStatus.ConditionalApproval.ToString();
    }

    public void PutOnProbation()
    {
        Status = SupplierStatus.OnProbation.ToString();
    }

    public void Disqualify()
    {
        Status = SupplierStatus.Disqualified.ToString();
    }

    public void Deactivate()
    {
        Status = SupplierStatus.Inactive.ToString();
    }

    public void AssessRisk(RiskLevel level, string? contributingFactors)
    {
        RiskAssessment = new SupplierRiskAssessment(level, contributingFactors?.Trim(), DateTime.UtcNow);
        RiskLevel = level.ToString();
    }

    public SupplierApproval AddApproval(
        string scopeDescription,
        DateTime approvedDate,
        DateTime? expiryDate,
        string? conditions)
    {
        var approval = SupplierApproval.Create(
            Guid.NewGuid(), TenantId, Id, scopeDescription, approvedDate, expiryDate, conditions);

        _approvals.Add(approval);
        return approval;
    }

    public SupplierScorecard CreateScorecard(
        DateTime periodStart,
        DateTime periodEnd,
        decimal qualityScore,
        decimal deliveryScore,
        decimal responsivenessScore,
        decimal costScore)
    {
        var scorecard = SupplierScorecard.Create(
            Guid.NewGuid(), TenantId, Id, periodStart, periodEnd,
            qualityScore, deliveryScore, responsivenessScore, costScore);

        _scorecards.Add(scorecard);

        AddDomainEvent(new SupplierScoreUpdatedEvent(
            Id, scorecard.OverallScore, periodStart, periodEnd, TenantId.Value));

        return scorecard;
    }

    public SCARRecord IssueScar(
        string scarNumber,
        Guid? nonConformanceId,
        string defectDescription,
        string severity,
        int responseDays = 14)
    {
        var scar = SCARRecord.Create(
            Guid.NewGuid(), TenantId, Id, scarNumber, nonConformanceId,
            defectDescription, severity, DateTime.UtcNow, responseDays);

        scar.SendToSupplier();
        _scars.Add(scar);
        return scar;
    }

    public void RespondToScar(Guid scarId, string rootCause, string correctiveActions, string? evidenceRefs)
    {
        var scar = FindScar(scarId);
        scar.SubmitResponse(rootCause, correctiveActions, evidenceRefs);
    }

    public void AcceptScarResponse(Guid scarId)
    {
        var scar = FindScar(scarId);
        scar.Accept();
    }

    public void RejectScarResponse(Guid scarId)
    {
        var scar = FindScar(scarId);
        scar.Reject();
    }

    public void RequireFollowUpOnScar(Guid scarId)
    {
        var scar = FindScar(scarId);
        scar.RequireFollowUp();
    }

    public void CloseScar(Guid scarId)
    {
        var scar = FindScar(scarId);
        scar.Close();
    }

    public void ReissueScar(Guid scarId)
    {
        var scar = FindScar(scarId);
        scar.Reissue();
    }

    public ApprovedPart AddApprovedPart(Guid partId, string? revisionScope, DateTime approvalDate)
    {
        var existing = _approvedParts.FirstOrDefault(ap => ap.PartId == partId && ap.IsActive);
        if (existing is not null)
            throw new DomainException("Part is already on the approved list for this supplier.");

        var approved = ApprovedPart.Create(Guid.NewGuid(), TenantId, Id, partId, revisionScope, approvalDate);
        _approvedParts.Add(approved);
        return approved;
    }

    private SCARRecord FindScar(Guid scarId)
    {
        return _scars.FirstOrDefault(s => s.Id == scarId)
            ?? throw new DomainException("SCAR record not found.");
    }
}
