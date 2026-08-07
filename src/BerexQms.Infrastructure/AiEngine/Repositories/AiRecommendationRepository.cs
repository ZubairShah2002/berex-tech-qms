using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.AiEngine.Repositories;

public sealed class AiRecommendationRepository : RepositoryBase<AiRecommendation>, IAiRecommendationRepository
{
    public AiRecommendationRepository(QmsDbContext context) : base(context) { }

    public async Task<IReadOnlyList<AiRecommendation>> GetByModuleAsync(
        string relatedModule, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.RelatedModule == relatedModule)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiRecommendation>> GetByTypeAsync(
        string recommendationType, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.RecommendationType == recommendationType)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiRecommendation>> GetByStatusAsync(
        string status, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.Status == status)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiRecommendation>> GetPendingReviewAsync(
        CancellationToken cancellationToken = default)
    {
        var generated = AiRecommendationStatus.Generated.ToString();
        var reviewed = AiRecommendationStatus.Reviewed.ToString();

        return await DbSet
            .Where(r => r.Status == generated || r.Status == reviewed)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiRecommendation>> GetBySeverityAsync(
        string severity, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.Severity == severity)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
