using BerexQms.Domain.SupplierQuality.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.SupplierQuality.Entities;

public sealed class SupplierScorecard : Entity<Guid>
{
    public Guid SupplierId { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public decimal QualityScore { get; private set; }
    public decimal DeliveryScore { get; private set; }
    public decimal ResponsivenessScore { get; private set; }
    public decimal CostScore { get; private set; }
    public decimal OverallScore { get; private set; }
    public string Status { get; private set; } = string.Empty;

    private SupplierScorecard() { }

    internal static SupplierScorecard Create(
        Guid id,
        TenantId tenantId,
        Guid supplierId,
        DateTime periodStart,
        DateTime periodEnd,
        decimal qualityScore,
        decimal deliveryScore,
        decimal responsivenessScore,
        decimal costScore)
    {
        if (periodEnd <= periodStart)
            throw new DomainException("Period end must be after period start.");

        var overall = (qualityScore * 0.40m)
                    + (deliveryScore * 0.25m)
                    + (responsivenessScore * 0.20m)
                    + (costScore * 0.15m);

        return new SupplierScorecard
        {
            Id = id,
            TenantId = tenantId,
            SupplierId = supplierId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            QualityScore = qualityScore,
            DeliveryScore = deliveryScore,
            ResponsivenessScore = responsivenessScore,
            CostScore = costScore,
            OverallScore = Math.Round(overall, 2),
            Status = ScorecardStatus.Draft.ToString(),
        };
    }

    internal void Publish()
    {
        if (Status == ScorecardStatus.Published.ToString())
            throw new DomainException("Scorecard is already published.");

        Status = ScorecardStatus.Published.ToString();
    }
}
