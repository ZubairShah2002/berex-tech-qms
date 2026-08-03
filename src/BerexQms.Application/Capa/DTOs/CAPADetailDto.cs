namespace BerexQms.Application.Capa.DTOs;

public sealed record CAPADetailDto(
    Guid Id,
    string CapaNumber,
    string Title,
    string Description,
    string Status,
    string Priority,
    CAPASourceDto Source,
    string OwnerId,
    string? AssignedTo,
    Guid? SourceNonConformanceId,
    DateTime? TargetClosureDate,
    DateTime? ClosedAt,
    string? ClosedBy,
    string? ClosureNotes,
    RootCauseAnalysisDto? RootCauseAnalysis,
    IReadOnlyList<CapaActionDto> Actions,
    IReadOnlyList<EffectivenessVerificationDto> Verifications,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? ModifiedAt);
