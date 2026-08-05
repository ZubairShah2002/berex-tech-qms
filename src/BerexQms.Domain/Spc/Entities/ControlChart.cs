using BerexQms.Domain.Spc.Enums;
using BerexQms.Domain.Spc.Events;
using BerexQms.Domain.Spc.ValueObjects;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Spc.Entities;

public sealed class ControlChart : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<DataPoint> _dataPoints = [];

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string ChartType { get; private set; } = string.Empty;
    public Guid PartId { get; private set; }
    public string CharacteristicName { get; private set; } = string.Empty;
    public int SubgroupSize { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public ControlLimits? ControlLimits { get; private set; }
    public ProcessCapability? ProcessCapability { get; private set; }
    public decimal? UpperSpecLimit { get; private set; }
    public decimal? LowerSpecLimit { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<DataPoint> DataPoints => _dataPoints.AsReadOnly();

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private ControlChart() { }

    public static ControlChart Create(
        Guid id,
        TenantId tenantId,
        string code,
        string name,
        ChartType chartType,
        Guid partId,
        string characteristicName,
        int subgroupSize,
        decimal? upperSpecLimit,
        decimal? lowerSpecLimit)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Control chart code is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Control chart name is required.");
        if (partId == Guid.Empty)
            throw new DomainException("Part is required for a control chart.");
        if (string.IsNullOrWhiteSpace(characteristicName))
            throw new DomainException("Characteristic name is required.");

        ValidateSubgroupSize(chartType, subgroupSize);
        ValidateSpecLimits(upperSpecLimit, lowerSpecLimit);

        return new ControlChart
        {
            Id = id,
            TenantId = tenantId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            ChartType = chartType.ToString(),
            PartId = partId,
            CharacteristicName = characteristicName.Trim(),
            SubgroupSize = subgroupSize,
            Status = ChartStatus.Active.ToString(),
            UpperSpecLimit = upperSpecLimit,
            LowerSpecLimit = lowerSpecLimit,
            IsActive = true,
        };
    }

    public void Update(
        string name,
        int subgroupSize,
        decimal? upperSpecLimit,
        decimal? lowerSpecLimit)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Control chart name is required.");

        ValidateSubgroupSize(GetChartType(), subgroupSize);
        ValidateSpecLimits(upperSpecLimit, lowerSpecLimit);

        Name = name.Trim();
        SubgroupSize = subgroupSize;
        UpperSpecLimit = upperSpecLimit;
        LowerSpecLimit = lowerSpecLimit;
    }

    public DataPoint AddDataPoint(
        decimal value,
        string? subgroupValues,
        int sampleSize,
        DateTime timestamp,
        Guid? inspectionId)
    {
        if (sampleSize < 1)
            throw new DomainException("Sample size must be at least 1.");

        var dataPoint = DataPoint.Create(
            Guid.NewGuid(), TenantId, Id, value, subgroupValues, sampleSize, timestamp, inspectionId);

        _dataPoints.Add(dataPoint);

        if (ControlLimits is not null)
        {
            if (value > ControlLimits.UpperControlLimit || value < ControlLimits.LowerControlLimit)
            {
                var violation = ControlRuleViolation.Rule1_BeyondThreeSigma.ToString();
                dataPoint.MarkViolation(violation);

                AddDomainEvent(new SPCViolationDetectedEvent(
                    Id,
                    dataPoint.Id,
                    violation,
                    value,
                    ControlLimits.UpperControlLimit,
                    ControlLimits.LowerControlLimit));
            }
        }

        return dataPoint;
    }

    public void SetControlLimits(decimal upperControlLimit, decimal centerLine, decimal lowerControlLimit)
    {
        if (upperControlLimit <= lowerControlLimit)
            throw new DomainException("Upper control limit must be greater than the lower control limit.");

        ControlLimits = new ControlLimits(
            upperControlLimit, centerLine, lowerControlLimit, UpperSpecLimit, LowerSpecLimit);
    }

    public void RecalculateCapability(
        decimal cp,
        decimal cpk,
        decimal pp,
        decimal ppk,
        decimal mean,
        decimal stdDev,
        int sampleSize)
    {
        if (sampleSize < 1)
            throw new DomainException("Sample size must be at least 1 to calculate process capability.");

        ProcessCapability = new ProcessCapability(cp, cpk, pp, ppk, mean, stdDev, sampleSize, DateTime.UtcNow);
    }

    public void Deactivate()
    {
        Status = ChartStatus.Inactive.ToString();
        IsActive = false;
    }

    public void Activate()
    {
        Status = ChartStatus.Active.ToString();
        IsActive = true;
    }

    public void MarkUnderReview()
    {
        Status = ChartStatus.UnderReview.ToString();
    }

    private ChartType GetChartType() => Enum.Parse<ChartType>(ChartType);

    private static void ValidateSubgroupSize(ChartType chartType, int subgroupSize)
    {
        if (subgroupSize < 1)
            throw new DomainException("Subgroup size must be at least 1.");

        switch (chartType)
        {
            case Enums.ChartType.IndividualMovingRange:
                if (subgroupSize != 1)
                    throw new DomainException("Individual/Moving Range charts require a subgroup size of 1.");
                break;
            case Enums.ChartType.XBarR:
            case Enums.ChartType.XBarS:
                if (subgroupSize is < 2 or > 10)
                    throw new DomainException("X-bar charts require a subgroup size between 2 and 10.");
                break;
        }
    }

    private static void ValidateSpecLimits(decimal? upperSpecLimit, decimal? lowerSpecLimit)
    {
        if (upperSpecLimit.HasValue && lowerSpecLimit.HasValue && upperSpecLimit.Value <= lowerSpecLimit.Value)
            throw new DomainException("Upper specification limit must be greater than the lower specification limit.");
    }
}
