using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Events;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AiEngine.Entities;

/// <summary>
/// A structured knowledge document that captures QMS business context from a
/// specific source module and entity. These documents form the knowledge
/// foundation for AI-powered analysis, recommendations, and retrieval.
/// </summary>
public sealed class AiContextDocument : AggregateRoot<Guid>, IAuditableEntity
{
    public string SourceModule { get; private set; } = string.Empty;
    public string? SourceEntityId { get; private set; }
    public string ContextType { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? MetadataJson { get; private set; }
    public string EmbeddingStatus { get; private set; } = string.Empty;
    public DateTime? IndexedAt { get; private set; }
    public string? IndexError { get; private set; }
    public int ContentVersion { get; private set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private AiContextDocument() { }

    public static AiContextDocument Create(
        Guid id,
        TenantId tenantId,
        string sourceModule,
        string? sourceEntityId,
        AiContextType contextType,
        string title,
        string content,
        string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(sourceModule))
            throw new DomainException("Source module is required.");
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Context document title is required.");
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Context document content is required.");

        return new AiContextDocument
        {
            Id = id,
            TenantId = tenantId,
            SourceModule = sourceModule.Trim(),
            SourceEntityId = sourceEntityId?.Trim(),
            ContextType = contextType.ToString(),
            Title = title.Trim(),
            Content = content,
            MetadataJson = metadataJson,
            EmbeddingStatus = AiEmbeddingStatus.Pending.ToString(),
            ContentVersion = 1,
        };
    }

    public void UpdateContent(string title, string content, string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Context document title is required.");
        if (string.IsNullOrWhiteSpace(content))
            throw new DomainException("Context document content is required.");

        Title = title.Trim();
        Content = content;
        MetadataJson = metadataJson;
        ContentVersion++;

        // Content changed — mark as stale so it gets re-indexed
        if (EmbeddingStatus == AiEmbeddingStatus.Indexed.ToString())
        {
            EmbeddingStatus = AiEmbeddingStatus.Stale.ToString();
        }
    }

    public void MarkProcessing()
    {
        if (EmbeddingStatus != AiEmbeddingStatus.Pending.ToString() &&
            EmbeddingStatus != AiEmbeddingStatus.Failed.ToString() &&
            EmbeddingStatus != AiEmbeddingStatus.Stale.ToString())
            throw new DomainException("Only pending, failed, or stale documents can be processed.");

        EmbeddingStatus = AiEmbeddingStatus.Processing.ToString();
        IndexError = null;
    }

    public void MarkIndexed()
    {
        if (EmbeddingStatus != AiEmbeddingStatus.Processing.ToString())
            throw new DomainException("Only a processing document can be marked as indexed.");

        EmbeddingStatus = AiEmbeddingStatus.Indexed.ToString();
        IndexedAt = DateTime.UtcNow;
        IndexError = null;

        AddDomainEvent(new AiContextDocumentIndexedEvent(Id, ContextType, SourceModule));
    }

    public void MarkFailed(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new DomainException("Error detail is required when marking a document as failed.");

        EmbeddingStatus = AiEmbeddingStatus.Failed.ToString();
        IndexError = error;
    }

    public void MarkStale()
    {
        if (EmbeddingStatus != AiEmbeddingStatus.Indexed.ToString())
            throw new DomainException("Only an indexed document can be marked as stale.");

        EmbeddingStatus = AiEmbeddingStatus.Stale.ToString();
    }
}
