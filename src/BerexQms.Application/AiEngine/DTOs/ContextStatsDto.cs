namespace BerexQms.Application.AiEngine.DTOs;

public sealed record ContextStatsDto(
    int TotalDocuments,
    int IndexedDocuments,
    int PendingDocuments,
    int FailedDocuments,
    int StaleDocuments,
    int ActiveSources,
    int TotalSources,
    DateTime? LastSyncedAt);
