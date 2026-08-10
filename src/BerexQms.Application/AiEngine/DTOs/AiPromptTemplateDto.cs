namespace BerexQms.Application.AiEngine.DTOs;

public sealed record AiPromptTemplateDto
{
    public Guid Id { get; init; }
    public string TaskType { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int Version { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
}
