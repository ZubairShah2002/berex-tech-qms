using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Events;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AiEngine.Entities;

/// <summary>
/// An AI-generated quality recommendation based on analysis of QMS context
/// documents and knowledge sources. Every recommendation is explainable,
/// traceable, and auditable. Human review is required before action.
/// </summary>
public sealed class AiRecommendation : AggregateRoot<Guid>, IAuditableEntity
{
    public string RecommendationType { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Severity { get; private set; } = string.Empty;
    public string? SourceContextIds { get; private set; }
    public string RelatedModule { get; private set; } = string.Empty;
    public string? RelatedEntityId { get; private set; }
    public decimal ConfidenceScore { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public string? SupportingData { get; private set; }
    public string? RecommendedAction { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewedBy { get; private set; }
    public string? ReviewNotes { get; private set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private AiRecommendation() { }

    public static AiRecommendation Create(
        Guid id,
        TenantId tenantId,
        AiRecommendationType recommendationType,
        string title,
        string description,
        AiSeverity severity,
        string relatedModule,
        string? relatedEntityId,
        decimal confidenceScore,
        string reason,
        string? supportingData,
        string? recommendedAction,
        string? sourceContextIds)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Recommendation title is required.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Recommendation description is required.");
        if (string.IsNullOrWhiteSpace(relatedModule))
            throw new DomainException("Related module is required.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Recommendation reason is required.");
        if (confidenceScore < 0 || confidenceScore > 1)
            throw new DomainException("Confidence score must be between 0 and 1.");

        var recommendation = new AiRecommendation
        {
            Id = id,
            TenantId = tenantId,
            RecommendationType = recommendationType.ToString(),
            Title = title.Trim(),
            Description = description.Trim(),
            Severity = severity.ToString(),
            RelatedModule = relatedModule.Trim(),
            RelatedEntityId = relatedEntityId?.Trim(),
            ConfidenceScore = confidenceScore,
            Status = AiRecommendationStatus.Generated.ToString(),
            Reason = reason.Trim(),
            SupportingData = supportingData,
            RecommendedAction = recommendedAction?.Trim(),
            SourceContextIds = sourceContextIds,
        };

        recommendation.AddDomainEvent(new AiRecommendationCreatedEvent(
            id,
            recommendationType.ToString(),
            severity.ToString(),
            relatedModule));

        return recommendation;
    }

    public void MarkReviewed(string reviewedBy)
    {
        if (Status != AiRecommendationStatus.Generated.ToString())
            throw new DomainException("Only generated recommendations can be marked as reviewed.");
        if (string.IsNullOrWhiteSpace(reviewedBy))
            throw new DomainException("Reviewer identity is required.");

        Status = AiRecommendationStatus.Reviewed.ToString();
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewedBy;

        AddDomainEvent(new AiRecommendationReviewedEvent(Id, Status, reviewedBy));
    }

    public void Accept(string reviewedBy, string? notes)
    {
        if (Status != AiRecommendationStatus.Generated.ToString() &&
            Status != AiRecommendationStatus.Reviewed.ToString())
            throw new DomainException("Only generated or reviewed recommendations can be accepted.");
        if (string.IsNullOrWhiteSpace(reviewedBy))
            throw new DomainException("Reviewer identity is required.");

        Status = AiRecommendationStatus.Accepted.ToString();
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewedBy;
        ReviewNotes = notes?.Trim();

        AddDomainEvent(new AiRecommendationReviewedEvent(Id, Status, reviewedBy));
    }

    public void Reject(string reviewedBy, string? notes)
    {
        if (Status != AiRecommendationStatus.Generated.ToString() &&
            Status != AiRecommendationStatus.Reviewed.ToString())
            throw new DomainException("Only generated or reviewed recommendations can be rejected.");
        if (string.IsNullOrWhiteSpace(reviewedBy))
            throw new DomainException("Reviewer identity is required.");

        Status = AiRecommendationStatus.Rejected.ToString();
        ReviewedAt = DateTime.UtcNow;
        ReviewedBy = reviewedBy;
        ReviewNotes = notes?.Trim();

        AddDomainEvent(new AiRecommendationReviewedEvent(Id, Status, reviewedBy));
    }

    public void MarkExpired()
    {
        if (Status == AiRecommendationStatus.Accepted.ToString() ||
            Status == AiRecommendationStatus.Rejected.ToString() ||
            Status == AiRecommendationStatus.Expired.ToString())
            throw new DomainException("Cannot expire a recommendation that has already been resolved.");

        Status = AiRecommendationStatus.Expired.ToString();
    }
}
