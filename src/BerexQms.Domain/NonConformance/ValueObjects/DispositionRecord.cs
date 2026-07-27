using BerexQms.Domain.NonConformance.Enums;

namespace BerexQms.Domain.NonConformance.ValueObjects;

public sealed record DispositionRecord(
    NCDispositionType Type,
    string Justification,
    string ApprovedBy,
    DateTime ApprovedAt);
