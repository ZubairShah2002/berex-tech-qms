namespace BerexQms.Application.Inspection.DTOs;

public sealed record GateResultDto(
    string GateType,
    bool Passed,
    string? Detail,
    DateTime CheckedAt);
