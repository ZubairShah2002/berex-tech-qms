using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.SearchKnowledgeContext;

internal sealed class SearchKnowledgeContextQueryHandler
    : IQueryHandler<SearchKnowledgeContextQuery, IReadOnlyList<ContextSearchResultDto>>
{
    private readonly IAiContextDocumentRepository _repository;

    public SearchKnowledgeContextQueryHandler(IAiContextDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<ContextSearchResultDto>>> Handle(
        SearchKnowledgeContextQuery request, CancellationToken cancellationToken)
    {
        var documents = await _repository.SearchByContentAsync(
            request.SearchTerm, cancellationToken);

        // Apply optional module filter
        if (!string.IsNullOrWhiteSpace(request.SourceModule))
        {
            documents = documents
                .Where(d => d.SourceModule.Equals(
                    request.SourceModule, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var results = documents
            .Take(request.MaxResults)
            .Select((doc, index) => MapToSearchResult(doc, documents.Count, index))
            .ToList();

        return results;
    }

    private static ContextSearchResultDto MapToSearchResult(
        AiContextDocument doc, int totalResults, int rank)
    {
        // Simple relevance scoring — position-based for keyword search.
        // A future vector-search implementation will replace this with cosine similarity.
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
