using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Repositories;

public interface IAiRecommendationRepository : IRepository<AiRecommendation>
{
    Task<IReadOnlyList<AiRecommendation>> GetByModuleAsync(
        string relatedModule, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiRecommendation>> GetByTypeAsync(
        string recommendationType, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiRecommendation>> GetByStatusAsync(
        string status, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiRecommendation>> GetPendingReviewAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiRecommendation>> GetBySeverityAsync(
        string severity, CancellationToken cancellationToken = default);
}
