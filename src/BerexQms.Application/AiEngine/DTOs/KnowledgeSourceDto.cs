namespace BerexQms.Application.AiEngine.DTOs;

public sealed record KnowledgeSourceDto(
    Guid Id,
    string Name,
    string Module,
    string? Description,
    bool IsActive,
    DateTime? LastSyncedAt,
    int DocumentCount,
    DateTime CreatedAt);
