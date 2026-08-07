using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetRecommendations;

internal sealed class GetRecommendationsQueryHandler
    : IQueryHandler<GetRecommendationsQuery, IReadOnlyList<AiRecommendationDto>>
{
    private readonly IAiRecommendationRepository _repository;

    public GetRecommendationsQueryHandler(IAiRecommendationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<AiRecommendationDto>>> Handle(
        GetRecommendationsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<AiRecommendation> recommendations;

        // Apply the most specific filter available
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            recommendations = await _repository.GetByStatusAsync(request.Status, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.RelatedModule))
        {
            recommendations = await _repository.GetByModuleAsync(request.RelatedModule, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.RecommendationType))
        {
            recommendations = await _repository.GetByTypeAsync(request.RecommendationType, cancellationToken);
        }
        else if (!string.IsNullOrWhiteSpace(request.Severity))
        {
            recommendations = await _repository.GetBySeverityAsync(request.Severity, cancellationToken);
        }
        else
        {
            recommendations = await _repository.ListAllAsync(cancellationToken);
        }

        // Apply secondary filters
        var filtered = recommendations.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.RecommendationType) &&
            !string.IsNullOrWhiteSpace(request.Status))
        {
            filtered = filtered.Where(r =>
                r.RecommendationType.Equals(request.RecommendationType, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.RelatedModule) &&
            !string.IsNullOrWhiteSpace(request.Status))
        {
            filtered = filtered.Where(r =>
                r.RelatedModule.Equals(request.RelatedModule, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.Severity) &&
            (!string.IsNullOrWhiteSpace(request.Status) ||
             !string.IsNullOrWhiteSpace(request.RelatedModule) ||
             !string.IsNullOrWhiteSpace(request.RecommendationType)))
        {
            filtered = filtered.Where(r =>
                r.Severity.Equals(request.Severity, StringComparison.OrdinalIgnoreCase));
        }

        var results = filtered
            .OrderByDescending(r => r.CreatedAt)
            .Select(MapToDto)
            .ToList();

        return results;
    }

    private static AiRecommendationDto MapToDto(AiRecommendation r) =>
        new(r.Id,
            r.RecommendationType,
            r.Title,
            r.Description,
            r.Severity,
            r.SourceContextIds,
            r.RelatedModule,
            r.RelatedEntityId,
            r.ConfidenceScore,
            r.Status,
            r.Reason,
            r.SupportingData,
            r.RecommendedAction,
            r.ReviewedAt,
            r.ReviewedBy,
            r.ReviewNotes,
            r.CreatedAt,
            r.ModifiedAt);
}
