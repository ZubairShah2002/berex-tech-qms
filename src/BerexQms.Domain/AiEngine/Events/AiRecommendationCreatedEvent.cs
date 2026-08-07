using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Events;

public sealed record AiRecommendationCreatedEvent(
    Guid RecommendationId,
    string RecommendationType,
    string Severity,
    string RelatedModule) : DomainEvent;
