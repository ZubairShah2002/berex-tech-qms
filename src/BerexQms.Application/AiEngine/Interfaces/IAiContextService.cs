using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Interfaces;

/// <summary>
/// Provides structured AI context retrieval and search across the QMS knowledge
/// foundation. Implemented in Infrastructure — reads from context documents and
/// knowledge sources to prepare context for AI analysis operations.
/// </summary>
public interface IAiContextService
{
    /// <summary>
    /// Retrieves the full context document for a specific source entity.
    /// Returns null when no context document exists for the given module/entity pair.
    /// </summary>
    Task<ContextDocumentDto?> GetContextAsync(
        string sourceModule, string sourceEntityId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a combined context payload from all relevant documents for a given
    /// module and optional context type filter. Returns structured content suitable
    /// for AI analysis operations.
    /// </summary>
    Task<IReadOnlyList<ContextDocumentDto>> BuildContextAsync(
        string sourceModule, string? contextType = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches indexed context documents by keyword, returning ranked results
    /// with relevance scores and content snippets.
    /// </summary>
    Task<IReadOnlyList<ContextSearchResultDto>> SearchRelevantContextAsync(
        string searchTerm, string? sourceModule = null, int maxResults = 20,
        CancellationToken cancellationToken = default);
}
