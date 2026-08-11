using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.AiEngine.Repositories;

internal sealed class AiUserPreferenceRepository
    : RepositoryBase<AiUserPreference>, IAiUserPreferenceRepository
{
    public AiUserPreferenceRepository(QmsDbContext context) : base(context) { }

    public async Task<AiUserPreference?> GetByUserIdAsync(
        Guid userId, CancellationToken ct)
    {
        return await DbSet
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);
    }
}
