using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetRiskSummary;

internal sealed class GetRiskSummaryQueryHandler
    : IQueryHandler<GetRiskSummaryQuery, RiskSummaryDto>
{
    private readonly IAiRecommendationRepository _repository;

    public GetRiskSummaryQueryHandler(IAiRecommendationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<RiskSummaryDto>> Handle(
        GetRiskSummaryQuery request, CancellationToken cancellationToken)
    {
        var all = await _repository.ListAllAsync(cancellationToken);

        var critical = AiSeverity.Critical.ToString();
        var high = AiSeverity.High.ToString();
        var medium = AiSeverity.Medium.ToString();
        var low = AiSeverity.Low.ToString();
        var generated = AiRecommendationStatus.Generated.ToString();
        var reviewed = AiRecommendationStatus.Reviewed.ToString();
        var accepted = AiRecommendationStatus.Accepted.ToString();
        var rejected = AiRecommendationStatus.Rejected.ToString();

        var riskByModule = all
            .GroupBy(r => r.RelatedModule)
            .Select(g => new RiskByModuleDto(
                g.Key,
                g.Count(),
                g.Count(r => r.Severity == critical),
                g.Count(r => r.Severity == high)))
            .OrderByDescending(m => m.CriticalCount)
            .ThenByDescending(m => m.HighCount)
            .ThenByDescending(m => m.Count)
            .ToList();

        var riskByType = all
            .GroupBy(r => r.RecommendationType)
            .Select(g => new RiskByTypeDto(
                g.Key,
                g.Count(),
                g.Average(r => r.ConfidenceScore)))
            .OrderByDescending(t => t.Count)
            .ToList();

        var summary = new RiskSummaryDto(
            TotalRecommendations: all.Count,
            CriticalCount: all.Count(r => r.Severity == critical),
            HighCount: all.Count(r => r.Severity == high),
            MediumCount: all.Count(r => r.Severity == medium),
            LowCount: all.Count(r => r.Severity == low),
            PendingReview: all.Count(r =>
                r.Status == generated || r.Status == reviewed),
            AcceptedCount: all.Count(r => r.Status == accepted),
            RejectedCount: all.Count(r => r.Status == rejected),
            RiskByModule: riskByModule,
            RiskByType: riskByType);

        return summary;
    }
}
