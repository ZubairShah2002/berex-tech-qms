namespace BerexQms.Domain.SupplierQuality.ValueObjects;

public sealed record ScarResponse(
    string RootCause,
    string CorrectiveActions,
    string? EvidenceRefs,
    DateTime ResponseDate);
