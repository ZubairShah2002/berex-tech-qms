using BerexQms.Domain.SupplierQuality.Enums;

namespace BerexQms.Domain.SupplierQuality.ValueObjects;

public sealed record SupplierRiskAssessment(
    RiskLevel Level,
    string? ContributingFactors,
    DateTime AssessedAt);
