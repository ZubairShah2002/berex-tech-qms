using BerexQms.Application.AiEngine.Interfaces;
using Microsoft.Extensions.Logging;

namespace BerexQms.Infrastructure.AiEngine.Services;

/// <summary>
/// Placeholder embedding service that returns empty/stub results.
/// A production implementation backed by a vector database (e.g., pgvector)
/// will replace this in a future sprint when RAG capabilities are activated.
/// </summary>
public sealed class EmbeddingService : IEmbeddingService
{
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(ILogger<EmbeddingService> logger)
    {
        _logger = logger;
    }

    public Task<float[]> GenerateEmbeddingAsync(
        string content, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Placeholder embedding generation invoked for content of length {ContentLength}",
            content.Length);

        // Return a zero vector — replaced by real model inference in a future sprint
        return Task.FromResult(new float[384]);
    }

    public Task StoreEmbeddingAsync(
        Guid documentId, float[] embedding, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Placeholder embedding storage invoked for document {DocumentId}, vector dimension {Dimension}",
            documentId, embedding.Length);

        // No-op — vector storage wired in a future sprint
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<(Guid DocumentId, decimal Score)>> SearchSimilarContextAsync(
        string queryText, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Placeholder similarity search invoked for query '{Query}', max results {MaxResults}",
            queryText, maxResults);

        // Return empty results — semantic search wired in a future sprint
        IReadOnlyList<(Guid DocumentId, decimal Score)> empty =
            Array.Empty<(Guid, decimal)>();

        return Task.FromResult(empty);
    }
}
