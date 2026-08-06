using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.AiEngine.Repositories;

public sealed class AiCapabilityConfigRepository : RepositoryBase<AiCapabilityConfig>, IAiCapabilityConfigRepository
{
    public AiCapabilityConfigRepository(QmsDbContext context) : base(context) { }

    public async Task<AiCapabilityConfig?> GetByCapabilityAsync(string capability, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(c => c.Capability == capability, cancellationToken);
    }

    public async Task<IReadOnlyList<AiCapabilityConfig>> GetAllConfigsAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.OrderBy(c => c.Capability).ToListAsync(cancellationToken);
    }
}
