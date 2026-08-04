using BerexQms.Domain.Calibration.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Calibration.Entities;

public sealed class ImpactAssessment : Entity<Guid>
{
    public Guid EquipmentId { get; private set; }
    public Guid FailedCalibrationId { get; private set; }
    public DateTime AffectedFrom { get; private set; }
    public DateTime AffectedTo { get; private set; }
    public int AffectedInspectionCount { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public Guid? ReviewedBy { get; private set; }
    public string? Notes { get; private set; }

    private ImpactAssessment() { }

    internal static ImpactAssessment Create(
        Guid id,
        TenantId tenantId,
        Guid equipmentId,
        Guid failedCalibrationId,
        DateTime affectedFrom,
        DateTime affectedTo,
        int affectedInspectionCount)
    {
        return new ImpactAssessment
        {
            Id = id,
            TenantId = tenantId,
            EquipmentId = equipmentId,
            FailedCalibrationId = failedCalibrationId,
            AffectedFrom = affectedFrom,
            AffectedTo = affectedTo,
            AffectedInspectionCount = affectedInspectionCount,
            Status = ImpactAssessmentStatus.Open.ToString(),
        };
    }

    public void StartReview(Guid reviewerId)
    {
        if (Status != ImpactAssessmentStatus.Open.ToString())
            throw new DomainException("Only open assessments can be reviewed.");

        Status = ImpactAssessmentStatus.UnderReview.ToString();
        ReviewedBy = reviewerId;
    }

    public void Close(string? notes)
    {
        if (Status != ImpactAssessmentStatus.UnderReview.ToString())
            throw new DomainException("Assessment must be under review before closing.");

        Status = ImpactAssessmentStatus.Closed.ToString();
        Notes = notes?.Trim();
    }
}
