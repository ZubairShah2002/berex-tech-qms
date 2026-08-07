namespace BerexQms.Application.AiEngine.DTOs;

public sealed record ContextSearchResultDto(
    Guid DocumentId,
    string SourceModule,
    string ContextType,
    string Title,
    string ContentSnippet,
    decimal RelevanceScore,
    DateTime? IndexedAt);
