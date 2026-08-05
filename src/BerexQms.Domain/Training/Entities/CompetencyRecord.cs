using BerexQms.Domain.Training.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Training.Entities;

public sealed class CompetencyRecord : Entity<Guid>
{
    public Guid EmployeeId { get; private set; }
    public Guid QualificationId { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTime? QualifiedDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public Guid? AssessorId { get; private set; }
    public string? EvidenceRef { get; private set; }

    private CompetencyRecord() { }

    public static CompetencyRecord Create(
        Guid id,
        TenantId tenantId,
        Guid employeeId,
        Guid qualificationId)
    {
        return new CompetencyRecord
        {
            Id = id,
            TenantId = tenantId,
            EmployeeId = employeeId,
            QualificationId = qualificationId,
            Status = QualificationStatus.NotStarted.ToString(),
        };
    }

    public void StartTraining()
    {
        if (Status != QualificationStatus.NotStarted.ToString() &&
            Status != QualificationStatus.Expired.ToString() &&
            Status != QualificationStatus.Suspended.ToString())
            throw new DomainException($"Cannot start training from status '{Status}'.");

        Status = QualificationStatus.InTraining.ToString();
    }

    public void MarkQualified(DateTime qualifiedDate, int validityMonths, Guid? assessorId, string? evidenceRef)
    {
        if (Status != QualificationStatus.InTraining.ToString())
            throw new DomainException($"Cannot qualify from status '{Status}'.");

        Status = QualificationStatus.Qualified.ToString();
        QualifiedDate = qualifiedDate;
        ExpiryDate = qualifiedDate.AddMonths(validityMonths);
        AssessorId = assessorId;
        EvidenceRef = evidenceRef;
    }

    public void MarkExpired()
    {
        if (Status != QualificationStatus.Qualified.ToString())
            throw new DomainException($"Cannot expire from status '{Status}'.");

        Status = QualificationStatus.Expired.ToString();
    }

    public void Suspend()
    {
        if (Status != QualificationStatus.Qualified.ToString())
            throw new DomainException($"Cannot suspend from status '{Status}'.");

        Status = QualificationStatus.Suspended.ToString();
    }

    public void Revoke()
    {
        if (Status != QualificationStatus.Qualified.ToString() &&
            Status != QualificationStatus.Suspended.ToString())
            throw new DomainException($"Cannot revoke from status '{Status}'.");

        Status = QualificationStatus.Revoked.ToString();
    }

    public bool IsWithinRenewalWindow(int renewalWindowDays)
    {
        if (Status != QualificationStatus.Qualified.ToString() || ExpiryDate is null)
            return false;

        return DateTime.UtcNow >= ExpiryDate.Value.AddDays(-renewalWindowDays);
    }
}
