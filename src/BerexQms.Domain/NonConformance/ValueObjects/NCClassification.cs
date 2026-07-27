namespace BerexQms.Domain.NonConformance.ValueObjects;

public sealed record NCClassification(
    string Category,
    string DefectType,
    string? DefectCode);
