using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.AiEngine.Repositories;

internal sealed class AiUsageRecordRepository
    : RepositoryBase<AiUsageRecord>, IAiUsageRecordRepository
{
    public AiUsageRecordRepository(QmsDbContext context) : base(context) { }

    public async Task<IReadOnlyList<AiUsageRecord>> GetByProviderAsync(
        string provider, CancellationToken cancellationToken)
    {
        return await DbSet
            .Where(r => r.Provider == provider)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiUsageRecord>> GetByTaskTypeAsync(
        string taskType, CancellationToken cancellationToken)
    {
        return await DbSet
            .Where(r => r.TaskType == taskType)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiUsageRecord>> GetRecentAsync(
        int count, CancellationToken cancellationToken)
    {
        return await DbSet
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}
