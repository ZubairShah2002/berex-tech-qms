using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Interfaces;

/// <summary>
/// Orchestrates AI reasoning requests. Selects the appropriate provider
/// based on task type and configuration, manages context retrieval,
/// handles failover, and records usage.
///
/// Flow:
/// Permission Check → Context Retrieval → Prompt Construction →
/// Provider Selection → AI Call → Response Validation →
/// Usage Recording → Return Normalized Result
/// </summary>
public interface IAiOrchestrator
{
    /// <summary>
    /// Execute an AI reasoning task end-to-end: retrieve context,
    /// select provider, call AI, validate response, record usage.
    /// </summary>
    Task<AiProviderResponseDto> ExecuteAsync(
        AiOrchestratorRequest request, CancellationToken cancellationToken = default);

    /// <summary>Get status of all configured providers.</summary>
    Task<IReadOnlyList<AiProviderStatusDto>> GetProviderStatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>Get current task-to-provider mappings.</summary>
    IReadOnlyList<AiTaskMappingDto> GetTaskMappings();
}

/// <summary>
/// Request to the AI orchestrator. Specifies task type, module context,
/// and optional content. The orchestrator handles provider selection,
/// context retrieval, prompt construction, and failover.
/// </summary>
public sealed record AiOrchestratorRequest
{
    /// <summary>The type of AI task to execute.</summary>
    public string TaskType { get; init; } = string.Empty;

    /// <summary>QMS module providing context (e.g., "Inspection", "SupplierQuality").</summary>
    public string? Module { get; init; }

    /// <summary>User-supplied content or question for analysis.</summary>
    public string? Content { get; init; }

    /// <summary>Optional entity ID for targeted analysis.</summary>
    public string? EntityId { get; init; }

    /// <summary>Maximum context documents to retrieve.</summary>
    public int MaxContextDocuments { get; init; } = 10;

    /// <summary>User ID making the request (for audit trail).</summary>
    public Guid? UserId { get; init; }
}
