using BerexQms.Domain.Calibration.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.Calibration.Entities;

public sealed class GaugeControl : Entity<Guid>
{
    public Guid EquipmentId { get; private set; }
    public Guid? CharacteristicId { get; private set; }
    public DateTime StudyDate { get; private set; }
    public decimal TotalGRRPct { get; private set; }
    public decimal RepeatabilityPct { get; private set; }
    public decimal ReproducibilityPct { get; private set; }
    public decimal? PartVariationPct { get; private set; }
    public int? Ndc { get; private set; }
    public string Result { get; private set; } = string.Empty;

    private GaugeControl() { }

    internal static GaugeControl Create(
        Guid id,
        TenantId tenantId,
        Guid equipmentId,
        Guid? characteristicId,
        DateTime studyDate,
        decimal totalGRRPct,
        decimal repeatabilityPct,
        decimal reproducibilityPct,
        decimal? partVariationPct,
        int? ndc)
    {
        if (totalGRRPct < 0 || totalGRRPct > 100)
            throw new DomainException("Total GRR percentage must be between 0 and 100.");

        var result = totalGRRPct switch
        {
            < 10m => GaugeRRResult.Acceptable,
            < 30m => GaugeRRResult.Marginal,
            _ => GaugeRRResult.Unacceptable
        };

        return new GaugeControl
        {
            Id = id,
            TenantId = tenantId,
            EquipmentId = equipmentId,
            CharacteristicId = characteristicId,
            StudyDate = studyDate,
            TotalGRRPct = totalGRRPct,
            RepeatabilityPct = repeatabilityPct,
            ReproducibilityPct = reproducibilityPct,
            PartVariationPct = partVariationPct,
            Ndc = ndc,
            Result = result.ToString(),
        };
    }
}
