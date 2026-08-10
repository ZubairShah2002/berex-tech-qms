using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AiEngine.Entities;

/// <summary>
/// Centralized prompt template. Avoids scattering prompt strings across handlers.
/// Templates include placeholders replaced at runtime with QMS context.
/// </summary>
public sealed class AiPromptTemplate : AggregateRoot<Guid>, IAuditableEntity
{
    public string TaskType { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string SystemPrompt { get; private set; } = string.Empty;
    public string UserPromptTemplate { get; private set; } = string.Empty;
    public string? OutputSchema { get; private set; }
    public bool IsActive { get; private set; }
    public int Version { get; private set; }

    // IAuditableEntity
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private AiPromptTemplate() { }

    public static AiPromptTemplate Create(
        Guid id,
        TenantId tenantId,
        string taskType,
        string name,
        string? description,
        string systemPrompt,
        string userPromptTemplate,
        string? outputSchema,
        int version = 1)
    {
        if (string.IsNullOrWhiteSpace(taskType))
            throw new DomainException("Task type is required.");
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Prompt template name is required.");
        if (string.IsNullOrWhiteSpace(systemPrompt))
            throw new DomainException("System prompt is required.");
        if (string.IsNullOrWhiteSpace(userPromptTemplate))
            throw new DomainException("User prompt template is required.");

        return new AiPromptTemplate
        {
            Id = id,
            TenantId = tenantId,
            TaskType = taskType,
            Name = name,
            Description = description,
            SystemPrompt = systemPrompt,
            UserPromptTemplate = userPromptTemplate,
            OutputSchema = outputSchema,
            IsActive = true,
            Version = version,
        };
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
