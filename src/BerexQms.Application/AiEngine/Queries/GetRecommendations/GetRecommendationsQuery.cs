using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.GetRecommendations;

public sealed record GetRecommendationsQuery(
    string? RecommendationType,
    string? Status,
    string? Severity,
    string? RelatedModule) : IQuery<IReadOnlyList<AiRecommendationDto>>;
