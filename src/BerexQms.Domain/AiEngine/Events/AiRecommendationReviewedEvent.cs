using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Events;

public sealed record AiRecommendationReviewedEvent(
    Guid RecommendationId,
    string Status,
    string ReviewedBy) : DomainEvent;
