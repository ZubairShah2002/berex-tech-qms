namespace BerexQms.Application.AiEngine.Interfaces;

/// <summary>
/// Abstraction for AI embedding generation and similarity search.
/// The initial implementation is a placeholder that returns empty results;
/// a production implementation backed by a vector database (e.g., pgvector)
/// will be wired in a future sprint when RAG capabilities are activated.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Generates an embedding vector for the given text content.
    /// Returns a float array representing the embedding.
    /// </summary>
    Task<float[]> GenerateEmbeddingAsync(
        string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores an embedding vector for a context document, keyed by document ID.
    /// </summary>
    Task StoreEmbeddingAsync(
        Guid documentId, float[] embedding, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for context documents similar to the given query text,
    /// returning document IDs ranked by cosine similarity.
    /// </summary>
    Task<IReadOnlyList<(Guid DocumentId, decimal Score)>> SearchSimilarContextAsync(
        string queryText, int maxResults = 10, CancellationToken cancellationToken = default);
}
