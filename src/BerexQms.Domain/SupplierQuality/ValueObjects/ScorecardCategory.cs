namespace BerexQms.Domain.SupplierQuality.ValueObjects;

public sealed record ScorecardCategory(
    string Name,
    decimal Weight,
    decimal Score);
