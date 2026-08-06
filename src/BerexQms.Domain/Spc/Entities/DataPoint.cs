using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Spc.Entities;

public sealed class DataPoint : Entity<Guid>
{
    public Guid ControlChartId { get; private set; }
    public decimal Value { get; private set; }
    public string? SubgroupValues { get; private set; }
    public int SampleSize { get; private set; }
    public DateTime Timestamp { get; private set; }
    public Guid? InspectionId { get; private set; }
    public string? RuleViolation { get; private set; }
    public bool IsOutOfControl { get; private set; }

    private DataPoint() { }

    internal static DataPoint Create(
        Guid id,
        TenantId tenantId,
        Guid controlChartId,
        decimal value,
        string? subgroupValues,
        int sampleSize,
        DateTime timestamp,
        Guid? inspectionId)
    {
        return new DataPoint
        {
            Id = id,
            TenantId = tenantId,
            ControlChartId = controlChartId,
            Value = value,
            SubgroupValues = subgroupValues?.Trim(),
            SampleSize = sampleSize,
            Timestamp = timestamp,
            InspectionId = inspectionId,
            IsOutOfControl = false,
        };
    }

    internal void MarkViolation(string ruleViolation)
    {
        IsOutOfControl = true;
        RuleViolation = ruleViolation;
    }
}
