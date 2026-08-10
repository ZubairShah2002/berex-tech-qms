using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetQualityInsights;

internal sealed class GetQualityInsightsQueryHandler
    : IQueryHandler<GetQualityInsightsQuery, IReadOnlyList<QualityInsightDto>>
{
    private readonly IAiRecommendationService _recommendationService;

    public GetQualityInsightsQueryHandler(IAiRecommendationService recommendationService)
    {
        _recommendationService = recommendationService;
    }

    public async Task<Result<IReadOnlyList<QualityInsightDto>>> Handle(
        GetQualityInsightsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<QualityInsightDto> insights;

        switch (request.AnalysisType?.ToLowerInvariant())
        {
            case "trend":
                insights = await _recommendationService.AnalyseQualityTrendAsync(
                    request.Module, cancellationToken);
                break;

            case "risk":
                insights = await _recommendationService.GenerateRiskAssessmentAsync(
                    request.Module, cancellationToken);
                break;

            default:
                insights = await _recommendationService.GenerateRecommendationsAsync(
                    request.Module, null, cancellationToken);
                break;
        }

        return insights.ToList();
    }
}
