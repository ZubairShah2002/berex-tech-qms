using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.AiEngine.Repositories;

public sealed class AiActionLogRepository : RepositoryBase<AiActionLog>, IAiActionLogRepository
{
    public AiActionLogRepository(QmsDbContext context) : base(context) { }

    public async Task<IReadOnlyList<AiActionLog>> GetByUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiActionLog>> GetPendingConfirmationsAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.UserId == userId && x.ConfirmationStatus == "Pending")
            .OrderByDescending(x => x.RequestedAt)
            .ToListAsync(cancellationToken);
    }
}
