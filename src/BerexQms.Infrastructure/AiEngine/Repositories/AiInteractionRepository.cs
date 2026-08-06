using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.AiEngine.Repositories;

public sealed class AiInteractionRepository : RepositoryBase<AiInteraction>, IAiInteractionRepository
{
    public AiInteractionRepository(QmsDbContext context) : base(context) { }

    public async Task<IReadOnlyList<AiInteraction>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.RequestedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiInteraction>> GetByCapabilityAsync(string capability, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(i => i.Capability == capability)
            .OrderByDescending(i => i.RequestedAt)
            .ToListAsync(cancellationToken);
    }
}
