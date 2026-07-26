using BerexQms.Domain.Inspection.Enums;

namespace BerexQms.Domain.Inspection.ValueObjects;

public sealed record LotDisposition(
    DispositionType Type,
    string Justification,
    string ApprovedBy,
    DateTime ApprovedAt);
