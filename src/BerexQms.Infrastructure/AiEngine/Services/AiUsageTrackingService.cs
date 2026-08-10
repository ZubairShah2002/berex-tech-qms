using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;

namespace BerexQms.Infrastructure.AiEngine.Services;

/// <summary>
/// Tracks AI usage per provider, model, and task type for cost management,
/// audit compliance, and dashboard reporting.
/// </summary>
internal sealed class AiUsageTrackingService : IAiUsageService
{
    private readonly IAiUsageRecordRepository _repository;
    private readonly ITenantContext _tenantContext;

    public AiUsageTrackingService(
        IAiUsageRecordRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task RecordUsageAsync(
        AiProviderResponseDto response,
        string taskType,
        Guid? userId,
        Guid? recommendationId,
        string? contextDocumentIds,
        bool wasFallback,
        string? fallbackFromProvider,
        CancellationToken cancellationToken)
    {
        var record = AiUsageRecord.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            response.Provider,
            response.Model,
            taskType,
            userId,
            response.TokenUsage.InputTokens,
            response.TokenUsage.OutputTokens,
            response.ProcessingTimeMs,
            response.Success,
            response.ErrorCategory,
            response.ErrorMessage,
            recommendationId,
            contextDocumentIds,
            wasFallback,
            fallbackFromProvider,
            response.TokenUsage.EstimatedCostUsd);

        await _repository.AddAsync(record, cancellationToken);
    }

    public async Task<AiUsageSummaryDto> GetUsageSummaryAsync(
        CancellationToken cancellationToken)
    {
        var records = await _repository.ListAllAsync(cancellationToken);

        if (records.Count == 0)
        {
            return new AiUsageSummaryDto
            {
                UsageByProvider = [],
                UsageByTaskType = [],
            };
        }

        var byProvider = records
            .GroupBy(r => r.Provider)
            .Select(g =>
            {
                var count = g.Count();
                var successCount = g.Count(r => r.Success);
                return new AiUsageByProviderDto
                {
                    Provider = g.Key,
                    RequestCount = count,
                    TotalTokens = g.Sum(r => r.TotalTokens),
                    EstimatedCostUsd = g.Sum(r => r.EstimatedCostUsd ?? 0),
                    AverageProcessingTimeMs = (long)g.Average(r => r.ProcessingTimeMs),
                    FailedCount = g.Count(r => !r.Success),
                    SuccessCount = successCount,
                    SuccessRate = count > 0 ? Math.Round((decimal)successCount / count * 100, 1) : 0,
                    FallbackCount = g.Count(r => r.WasFallback),
                };
            })
            .OrderByDescending(p => p.RequestCount)
            .ToList();

        var byTaskType = records
            .GroupBy(r => r.TaskType)
            .Select(g => new AiUsageByTaskTypeDto
            {
                TaskType = g.Key,
                RequestCount = g.Count(),
                TotalTokens = g.Sum(r => r.TotalTokens),
                AverageProcessingTimeMs = (long)g.Average(r => r.ProcessingTimeMs),
            })
            .OrderByDescending(t => t.RequestCount)
            .ToList();

        // Cost optimization metrics
        var localRecords = records.Where(r => r.Provider == "Local").ToList();
        var cloudRecords = records.Where(r => r.Provider != "Local").ToList();

        var localRequests = localRecords.Count;
        var cloudRequests = cloudRecords.Count;
        var totalRequests = records.Count;

        // Estimate avoided API cost: use the average cloud cost per token
        // applied to the tokens processed locally
        var estimatedAvoidedCost = 0m;
        if (cloudRecords.Count > 0 && localRecords.Count > 0)
        {
            var totalCloudTokens = cloudRecords.Sum(r => r.TotalTokens);
            var totalCloudCost = cloudRecords.Sum(r => r.EstimatedCostUsd ?? 0);
            var avgCostPerToken = totalCloudTokens > 0
                ? totalCloudCost / totalCloudTokens
                : 0;

            var localTokens = localRecords.Sum(r => r.TotalTokens);
            estimatedAvoidedCost = localTokens * avgCostPerToken;
        }

        return new AiUsageSummaryDto
        {
            TotalRequests = totalRequests,
            SuccessfulRequests = records.Count(r => r.Success),
            FailedRequests = records.Count(r => !r.Success),
            FallbackRequests = records.Count(r => r.WasFallback),
            TotalInputTokens = records.Sum(r => r.InputTokens),
            TotalOutputTokens = records.Sum(r => r.OutputTokens),
            TotalEstimatedCostUsd = records.Sum(r => r.EstimatedCostUsd ?? 0),
            AverageProcessingTimeMs = (long)records.Average(r => r.ProcessingTimeMs),
            UsageByProvider = byProvider,
            UsageByTaskType = byTaskType,
            LocalRequests = localRequests,
            CloudRequests = cloudRequests,
            LocalUsagePercent = totalRequests > 0 ? Math.Round((decimal)localRequests / totalRequests * 100, 1) : 0,
            CloudUsagePercent = totalRequests > 0 ? Math.Round((decimal)cloudRequests / totalRequests * 100, 1) : 0,
            EstimatedAvoidedApiCostUsd = Math.Round(estimatedAvoidedCost, 4),
        };
    }
}
