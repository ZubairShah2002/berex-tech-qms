using BerexQms.Domain.Inspection.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Inspection.Entities;

public sealed class SamplingPlan : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid PartId { get; private set; }
    public Guid? SupplierId { get; private set; }
    public InspectionType InspectionType { get; private set; }
    public SamplingLevel Level { get; private set; }
    public decimal AqlValue { get; private set; }
    public int SampleSize { get; private set; }
    public int AcceptNumber { get; private set; }
    public int RejectNumber { get; private set; }
    public bool IsActive { get; private set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private SamplingPlan() { }

    public static SamplingPlan Create(
        Guid id,
        TenantId tenantId,
        Guid partId,
        Guid? supplierId,
        InspectionType inspectionType,
        SamplingLevel level,
        decimal aqlValue,
        int sampleSize,
        int acceptNumber,
        int rejectNumber)
    {
        if (sampleSize <= 0)
            throw new DomainException("Sample size must be greater than zero.");

        if (acceptNumber < 0)
            throw new DomainException("Accept number cannot be negative.");

        if (rejectNumber <= acceptNumber)
            throw new DomainException("Reject number must be greater than accept number.");

        return new SamplingPlan
        {
            Id = id,
            TenantId = tenantId,
            PartId = partId,
            SupplierId = supplierId,
            InspectionType = inspectionType,
            Level = level,
            AqlValue = aqlValue,
            SampleSize = sampleSize,
            AcceptNumber = acceptNumber,
            RejectNumber = rejectNumber,
            IsActive = true
        };
    }

    public void Update(
        SamplingLevel level,
        decimal aqlValue,
        int sampleSize,
        int acceptNumber,
        int rejectNumber)
    {
        if (sampleSize <= 0)
            throw new DomainException("Sample size must be greater than zero.");

        if (acceptNumber < 0)
            throw new DomainException("Accept number cannot be negative.");

        if (rejectNumber <= acceptNumber)
            throw new DomainException("Reject number must be greater than accept number.");

        Level = level;
        AqlValue = aqlValue;
        SampleSize = sampleSize;
        AcceptNumber = acceptNumber;
        RejectNumber = rejectNumber;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Sampling plan is already inactive.");

        IsActive = false;
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("Sampling plan is already active.");

        IsActive = true;
    }
}
