using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.GetRecommendationDetails;

public sealed record GetRecommendationDetailsQuery(Guid RecommendationId)
    : IQuery<AiRecommendationDto>;
