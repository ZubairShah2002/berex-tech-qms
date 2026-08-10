using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Interfaces;

/// <summary>
/// Provider-independent AI interface. Implementations (Claude, OpenAI) live
/// in Infrastructure. Domain and Application layers know nothing about
/// API keys, HTTP clients, model names, or provider SDKs.
/// </summary>
public interface IAiProvider
{
    /// <summary>Provider identifier (e.g., "Claude", "OpenAi").</summary>
    string ProviderName { get; }

    /// <summary>Whether this provider is currently enabled and configured.</summary>
    bool IsEnabled { get; }

    /// <summary>General-purpose analysis of QMS context.</summary>
    Task<AiProviderResponseDto> AnalyzeAsync(
        AiProviderRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>Generate structured recommendations from QMS data.</summary>
    Task<AiProviderResponseDto> GenerateRecommendationAsync(
        AiProviderRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>Summarize QMS documents or data sets.</summary>
    Task<AiProviderResponseDto> SummarizeAsync(
        AiProviderRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>Explain a quality event, deviation, or process outcome.</summary>
    Task<AiProviderResponseDto> ExplainAsync(
        AiProviderRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>Extract structured data from unstructured QMS content.</summary>
    Task<AiProviderResponseDto> ExtractStructuredDataAsync(
        AiProviderRequestDto request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request to an AI provider. Contains the task context, system prompt,
/// user prompt, and any relevant context documents. No provider-specific
/// details are exposed.
/// </summary>
public sealed record AiProviderRequestDto
{
    public string TaskType { get; init; } = string.Empty;
    public string SystemPrompt { get; init; } = string.Empty;
    public string UserPrompt { get; init; } = string.Empty;
    public IReadOnlyList<AiContextSnippetDto> ContextDocuments { get; init; } = [];
    public string? OutputSchema { get; init; }
    public int? MaxTokens { get; init; }
    public decimal? Temperature { get; init; }
}

/// <summary>
/// A snippet of QMS context to include in an AI request. Ranked by relevance
/// and filtered for tenant isolation before reaching the provider.
/// </summary>
public sealed record AiContextSnippetDto
{
    public string DocumentId { get; init; } = string.Empty;
    public string SourceModule { get; init; } = string.Empty;
    public string ContextType { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public decimal RelevanceScore { get; init; }
}
