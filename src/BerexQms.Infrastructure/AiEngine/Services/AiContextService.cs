using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;

namespace BerexQms.Infrastructure.AiEngine.Services;

public sealed class AiContextService : IAiContextService
{
    private readonly IAiContextDocumentRepository _documentRepository;

    public AiContextService(IAiContextDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<ContextDocumentDto?> GetContextAsync(
        string sourceModule, string sourceEntityId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetBySourceEntityAsync(
            sourceModule, sourceEntityId, cancellationToken);

        return document is null ? null : MapToDto(document);
    }

    public async Task<IReadOnlyList<ContextDocumentDto>> BuildContextAsync(
        string sourceModule, string? contextType = null, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AiContextDocument> documents;

        if (!string.IsNullOrWhiteSpace(contextType))
        {
            documents = await _documentRepository.GetByContextTypeAsync(
                contextType, cancellationToken);

            documents = documents
                .Where(d => d.SourceModule.Equals(sourceModule, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
        else
        {
            documents = await _documentRepository.GetByModuleAsync(
                sourceModule, cancellationToken);
        }

        return documents.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<ContextSearchResultDto>> SearchRelevantContextAsync(
        string searchTerm, string? sourceModule = null, int maxResults = 20,
        CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.SearchByContentAsync(
            searchTerm, cancellationToken);

        if (!string.IsNullOrWhiteSpace(sourceModule))
        {
            documents = documents
                .Where(d => d.SourceModule.Equals(sourceModule, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return documents
            .Take(maxResults)
            .Select((doc, index) => MapToSearchResult(doc, documents.Count, index))
            .ToList();
    }

    private static ContextDocumentDto MapToDto(AiContextDocument doc)
    {
        return new ContextDocumentDto(
            doc.Id,
            doc.SourceModule,
            doc.SourceEntityId,
            doc.ContextType,
            doc.Title,
            doc.Content,
            doc.MetadataJson,
            doc.EmbeddingStatus,
            doc.IndexedAt,
            doc.IndexError,
            doc.ContentVersion,
            doc.CreatedAt,
            doc.ModifiedAt);
    }

    private static ContextSearchResultDto MapToSearchResult(
        AiContextDocument doc, int totalResults, int rank)
    {
        var relevanceScore = totalResults > 1
            ? Math.Round(1.0m - (rank / (decimal)totalResults), 4)
            : 1.0m;

        var snippet = doc.Content.Length > 300
            ? doc.Content[..300] + "…"
            : doc.Content;

        return new ContextSearchResultDto(
            doc.Id,
            doc.SourceModule,
            doc.ContextType,
            doc.Title,
            snippet,
            relevanceScore,
            doc.IndexedAt);
    }
}
