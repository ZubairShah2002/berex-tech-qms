namespace BerexQms.Application.AiEngine.DTOs;

public sealed record ContextDocumentDto(
    Guid Id,
    string SourceModule,
    string? SourceEntityId,
    string ContextType,
    string Title,
    string Content,
    string? MetadataJson,
    string EmbeddingStatus,
    DateTime? IndexedAt,
    string? IndexError,
    int ContentVersion,
    DateTime CreatedAt,
    DateTime? ModifiedAt);
