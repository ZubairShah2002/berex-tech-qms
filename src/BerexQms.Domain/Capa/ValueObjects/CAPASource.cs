using BerexQms.Domain.Capa.Enums;

namespace BerexQms.Domain.Capa.ValueObjects;

public sealed record CAPASource(
    CAPASourceType SourceType,
    Guid? SourceNonConformanceId,
    Guid? SourceAuditFindingId,
    string? SourceDescription);
