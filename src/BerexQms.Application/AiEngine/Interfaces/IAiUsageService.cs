using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Interfaces;

/// <summary>
/// Tracks and reports AI provider usage for cost management and auditing.
/// </summary>
public interface IAiUsageService
{
    Task RecordUsageAsync(
        AiProviderResponseDto response,
        string taskType,
        Guid? userId,
        Guid? recommendationId,
        string? contextDocumentIds,
        bool wasFallback,
        string? fallbackFromProvider,
        CancellationToken cancellationToken = default);

    Task<AiUsageSummaryDto> GetUsageSummaryAsync(
        CancellationToken cancellationToken = default);
}
