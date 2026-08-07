namespace BerexQms.Application.AiEngine.DTOs;

public sealed record AiWorkflowDefinitionDto(
    Guid Id,
    string Name,
    string? Description,
    string MinimumPermissionLevel,
    string Category,
    bool IsActive,
    string AffectedModules,
    DateTime CreatedAt);
