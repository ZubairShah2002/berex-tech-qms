using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetContextStats;

internal sealed class GetContextStatsQueryHandler
    : IQueryHandler<GetContextStatsQuery, ContextStatsDto>
{
    private readonly IAiContextDocumentRepository _documentRepository;
    private readonly IAiKnowledgeSourceRepository _sourceRepository;

    public GetContextStatsQueryHandler(
        IAiContextDocumentRepository documentRepository,
        IAiKnowledgeSourceRepository sourceRepository)
    {
        _documentRepository = documentRepository;
        _sourceRepository = sourceRepository;
    }

    public async Task<Result<ContextStatsDto>> Handle(
        GetContextStatsQuery request, CancellationToken cancellationToken)
    {
        var allDocuments = await _documentRepository.ListAllAsync(cancellationToken);
        var allSources = await _sourceRepository.ListAllAsync(cancellationToken);

        var indexed = AiEmbeddingStatus.Indexed.ToString();
        var pending = AiEmbeddingStatus.Pending.ToString();
        var failed = AiEmbeddingStatus.Failed.ToString();
        var stale = AiEmbeddingStatus.Stale.ToString();

        var stats = new ContextStatsDto(
            TotalDocuments: allDocuments.Count,
            IndexedDocuments: allDocuments.Count(d => d.EmbeddingStatus == indexed),
            PendingDocuments: allDocuments.Count(d => d.EmbeddingStatus == pending),
            FailedDocuments: allDocuments.Count(d => d.EmbeddingStatus == failed),
            StaleDocuments: allDocuments.Count(d => d.EmbeddingStatus == stale),
            ActiveSources: allSources.Count(s => s.IsActive),
            TotalSources: allSources.Count,
            LastSyncedAt: allSources
                .Where(s => s.LastSyncedAt.HasValue)
                .OrderByDescending(s => s.LastSyncedAt)
                .Select(s => s.LastSyncedAt)
                .FirstOrDefault());

        return stats;
    }
}
