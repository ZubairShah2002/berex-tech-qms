using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetCapabilityStats;

internal sealed class GetCapabilityStatsQueryHandler
    : IQueryHandler<GetCapabilityStatsQuery, AiCapabilityStatsDto>
{
    private readonly IAiInteractionRepository _repository;

    public GetCapabilityStatsQueryHandler(IAiInteractionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AiCapabilityStatsDto>> Handle(
        GetCapabilityStatsQuery request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiCapabilityType>(request.Capability, true, out var capability))
            return AiEngineErrors.InvalidCapability;

        var periodEnd = DateTime.UtcNow;
        var periodStart = periodEnd.AddDays(-request.Days);

        var spec = new InteractionsInPeriodSpec(capability, periodStart);
        var interactions = await _repository.ListAsync(spec, cancellationToken);

        var completedStatus = AiInteractionStatus.Completed.ToString();
        var failedStatus = AiInteractionStatus.Failed.ToString();
        var acceptedAction = AiUserAction.Accepted.ToString();
        var rejectedAction = AiUserAction.Rejected.ToString();

        var confidenceScores = interactions
            .Where(i => i.Confidence is not null)
            .Select(i => i.Confidence!.Score)
            .ToList();

        var responseTimes = interactions
            .Where(i => i.ResponseTimeMs.HasValue)
            .Select(i => i.ResponseTimeMs!.Value)
            .ToList();

        var stats = new AiCapabilityStatsDto(
            capability.ToString(),
            interactions.Count,
            interactions.Count(i => i.Status == completedStatus),
            interactions.Count(i => i.Status == failedStatus),
            interactions.Count(i => i.UserAction == acceptedAction),
            interactions.Count(i => i.UserAction == rejectedAction),
            confidenceScores.Count > 0 ? confidenceScores.Average() : 0m,
            responseTimes.Count > 0 ? responseTimes.Average() : 0d,
            $"{periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd} ({request.Days} days)");

        return stats;
    }

    /// <summary>
    /// Selects an AI capability's interactions requested on or after a cutoff date, with
    /// no paging — the results are consumed in memory to compute aggregate statistics.
    /// </summary>
    private sealed class InteractionsInPeriodSpec : Specification<AiInteraction>
    {
        public InteractionsInPeriodSpec(AiCapabilityType capability, DateTime periodStart)
        {
            var capabilityName = capability.ToString();
            ApplyCriteria(i => i.Capability == capabilityName && i.RequestedAt >= periodStart);
        }
    }
}
