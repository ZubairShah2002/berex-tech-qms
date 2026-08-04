namespace BerexQms.Application.SupplierQuality.DTOs;

public sealed record SupplierScorecardDto(
    Guid Id,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal QualityScore,
    decimal DeliveryScore,
    decimal ResponsivenessScore,
    decimal CostScore,
    decimal OverallScore,
    string Status);
