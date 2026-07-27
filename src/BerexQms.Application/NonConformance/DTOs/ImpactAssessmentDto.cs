namespace BerexQms.Application.NonConformance.DTOs;

public sealed record ImpactAssessmentDto(
    int AffectedQuantity,
    bool ShippedProductAffected,
    string? CustomerImpactDescription);
