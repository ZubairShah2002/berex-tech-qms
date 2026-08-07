using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.AiEngine.Repositories;

public sealed class AiPermissionPolicyRepository : RepositoryBase<AiPermissionPolicy>, IAiPermissionPolicyRepository
{
    public AiPermissionPolicyRepository(QmsDbContext context) : base(context) { }

    public async Task<AiPermissionPolicy?> GetActiveByUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive, cancellationToken);
    }
}
