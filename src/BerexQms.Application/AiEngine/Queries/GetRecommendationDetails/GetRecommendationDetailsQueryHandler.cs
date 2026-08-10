using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetRecommendationDetails;

internal sealed class GetRecommendationDetailsQueryHandler
    : IQueryHandler<GetRecommendationDetailsQuery, AiRecommendationDto>
{
    private readonly IAiRecommendationRepository _repository;

    public GetRecommendationDetailsQueryHandler(IAiRecommendationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AiRecommendationDto>> Handle(
        GetRecommendationDetailsQuery request, CancellationToken cancellationToken)
    {
        var recommendation = await _repository.GetByIdAsync(
            request.RecommendationId, cancellationToken);

        if (recommendation is null)
            return AiEngineErrors.RecommendationNotFound;

        return new AiRecommendationDto(
            recommendation.Id,
            recommendation.RecommendationType,
            recommendation.Title,
            recommendation.Description,
            recommendation.Severity,
            recommendation.SourceContextIds,
            recommendation.RelatedModule,
            recommendation.RelatedEntityId,
            recommendation.ConfidenceScore,
            recommendation.Status,
            recommendation.Reason,
            recommendation.SupportingData,
            recommendation.RecommendedAction,
            recommendation.ReviewedAt,
            recommendation.ReviewedBy,
            recommendation.ReviewNotes,
            recommendation.CreatedAt,
            recommendation.ModifiedAt);
    }
}
