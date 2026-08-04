using BerexQms.Domain.SupplierQuality.Enums;
using BerexQms.Domain.SupplierQuality.ValueObjects;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.SupplierQuality.Entities;

public sealed class SCARRecord : Entity<Guid>
{
    public Guid SupplierId { get; private set; }
    public string ScarNumber { get; private set; } = string.Empty;
    public Guid? NonConformanceId { get; private set; }
    public string DefectDescription { get; private set; } = string.Empty;
    public string Severity { get; private set; } = string.Empty;
    public DateTime IssuedDate { get; private set; }
    public DateTime ResponseDeadline { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public ScarResponse? Response { get; private set; }

    private SCARRecord() { }

    internal static SCARRecord Create(
        Guid id,
        TenantId tenantId,
        Guid supplierId,
        string scarNumber,
        Guid? nonConformanceId,
        string defectDescription,
        string severity,
        DateTime issuedDate,
        int responseDays = 14)
    {
        if (string.IsNullOrWhiteSpace(scarNumber))
            throw new DomainException("SCAR number is required.");

        if (string.IsNullOrWhiteSpace(defectDescription))
            throw new DomainException("Defect description is required.");

        return new SCARRecord
        {
            Id = id,
            TenantId = tenantId,
            SupplierId = supplierId,
            ScarNumber = scarNumber.Trim(),
            NonConformanceId = nonConformanceId,
            DefectDescription = defectDescription.Trim(),
            Severity = severity,
            IssuedDate = issuedDate,
            ResponseDeadline = issuedDate.AddDays(responseDays),
            Status = ScarStatus.Issued.ToString(),
        };
    }

    internal void SendToSupplier()
    {
        if (Status != ScarStatus.Issued.ToString())
            throw new DomainException("SCAR can only be sent when in Issued status.");

        Status = ScarStatus.AwaitingResponse.ToString();
    }

    internal void SubmitResponse(string rootCause, string correctiveActions, string? evidenceRefs)
    {
        if (Status != ScarStatus.AwaitingResponse.ToString()
            && Status != ScarStatus.Overdue.ToString())
            throw new DomainException("SCAR response can only be submitted when awaiting response or overdue.");

        if (string.IsNullOrWhiteSpace(rootCause))
            throw new DomainException("Root cause analysis is required.");

        if (string.IsNullOrWhiteSpace(correctiveActions))
            throw new DomainException("Corrective actions are required.");

        Response = new ScarResponse(rootCause.Trim(), correctiveActions.Trim(), evidenceRefs?.Trim(), DateTime.UtcNow);
        Status = ScarStatus.UnderReview.ToString();
    }

    internal void Accept()
    {
        if (Status != ScarStatus.UnderReview.ToString())
            throw new DomainException("SCAR can only be accepted when under review.");

        Status = ScarStatus.Accepted.ToString();
    }

    internal void Reject()
    {
        if (Status != ScarStatus.UnderReview.ToString() && Status != ScarStatus.FollowUp.ToString())
            throw new DomainException("SCAR can only be rejected when under review or follow-up.");

        Status = ScarStatus.Rejected.ToString();
    }

    internal void RequireFollowUp()
    {
        if (Status != ScarStatus.Accepted.ToString())
            throw new DomainException("Follow-up can only be required after acceptance.");

        Status = ScarStatus.FollowUp.ToString();
    }

    internal void Close()
    {
        if (Status != ScarStatus.FollowUp.ToString() && Status != ScarStatus.Accepted.ToString())
            throw new DomainException("SCAR can only be closed from accepted or follow-up status.");

        Status = ScarStatus.Closed.ToString();
    }

    internal void MarkOverdue()
    {
        if (Status == ScarStatus.AwaitingResponse.ToString())
            Status = ScarStatus.Overdue.ToString();
    }

    internal void Reissue()
    {
        if (Status != ScarStatus.Rejected.ToString())
            throw new DomainException("SCAR can only be reissued after rejection.");

        Response = null;
        Status = ScarStatus.AwaitingResponse.ToString();
    }
}
