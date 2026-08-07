using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AiEngine.Entities;

/// <summary>
/// Represents a registered knowledge source — a QMS module that contributes
/// structured context to the AI knowledge foundation. Tracks which modules
/// are active providers and their last synchronization timestamps.
/// </summary>
public sealed class AiKnowledgeSource : AggregateRoot<Guid>, IAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastSyncedAt { get; private set; }
    public int DocumentCount { get; private set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private AiKnowledgeSource() { }

    public static AiKnowledgeSource Create(
        Guid id,
        TenantId tenantId,
        string name,
        string module,
        string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Knowledge source name is required.");
        if (string.IsNullOrWhiteSpace(module))
            throw new DomainException("Knowledge source module is required.");

        return new AiKnowledgeSource
        {
            Id = id,
            TenantId = tenantId,
            Name = name.Trim(),
            Module = module.Trim(),
            Description = description?.Trim(),
            IsActive = true,
            DocumentCount = 0,
        };
    }

    public void Activate()
    {
        if (IsActive)
            throw new DomainException("Knowledge source is already active.");

        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Knowledge source is already inactive.");

        IsActive = false;
    }

    public void RecordSync(int documentCount)
    {
        if (documentCount < 0)
            throw new DomainException("Document count cannot be negative.");

        LastSyncedAt = DateTime.UtcNow;
        DocumentCount = documentCount;
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
    }
}
