namespace BerexQms.Domain.AiEngine.Enums;

/// <summary>
/// Tracks the embedding/indexing state of an AI context document.
/// </summary>
public enum AiEmbeddingStatus
{
    /// <summary>Document created but not yet queued for indexing.</summary>
    Pending = 1,

    /// <summary>Document is currently being processed for embedding generation.</summary>
    Processing = 2,

    /// <summary>Document has been successfully indexed and is searchable.</summary>
    Indexed = 3,

    /// <summary>Embedding generation or indexing failed.</summary>
    Failed = 4,

    /// <summary>Content has changed since last indexing; re-indexing required.</summary>
    Stale = 5,
}
