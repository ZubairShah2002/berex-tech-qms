namespace BerexQms.Application.NonConformance.DTOs;

public sealed record InvestigationDto(
    Guid Id,
    string InvestigatorId,
    string? Methodology,
    string? RootCause,
    string? Findings,
    DateTime StartedAt,
    DateTime? CompletedAt);
