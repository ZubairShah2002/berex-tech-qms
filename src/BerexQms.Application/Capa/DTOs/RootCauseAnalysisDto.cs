namespace BerexQms.Application.Capa.DTOs;

public sealed record RootCauseAnalysisDto(
    Guid Id,
    string Methodology,
    string? AnalysisDetails,
    string? RootCause,
    string? ContributingFactors,
    string AnalystId,
    DateTime StartedAt,
    DateTime? CompletedAt);
