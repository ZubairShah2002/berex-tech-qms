using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.AiEngine.Repositories;

public sealed class AiModelRepository : RepositoryBase<AiModel>, IAiModelRepository
{
    public AiModelRepository(QmsDbContext context) : base(context) { }

    public async Task<AiModel?> GetActiveModelAsync(string capability, CancellationToken cancellationToken = default)
    {
        var activeStatus = ModelStatus.Active.ToString();
        return await DbSet
            .Where(m => m.Capability == capability && m.Status == activeStatus)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> VersionExistsAsync(string name, string version, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();
        var normalizedVersion = version.Trim();
        return await DbSet.AnyAsync(
            m => m.Name == normalizedName && m.Version == normalizedVersion,
            cancellationToken);
    }
}
