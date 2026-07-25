using BerexQms.Domain.Inspection.Enums;

namespace BerexQms.Domain.Inspection.ValueObjects;

public sealed record IntegrityGateResult(
    GateType GateType,
    bool Passed,
    string? Detail,
    DateTime CheckedAt);
