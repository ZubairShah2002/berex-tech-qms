namespace BerexQms.Domain.NonConformance.ValueObjects;

public sealed record ImpactAssessment(
    int AffectedQuantity,
    bool ShippedProductAffected,
    string? CustomerImpactDescription);
