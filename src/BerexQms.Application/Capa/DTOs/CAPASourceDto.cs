namespace BerexQms.Application.Capa.DTOs;

public sealed record CAPASourceDto(
    string SourceType,
    Guid? SourceNonConformanceId,
    Guid? SourceAuditFindingId,
    string? SourceDescription);
