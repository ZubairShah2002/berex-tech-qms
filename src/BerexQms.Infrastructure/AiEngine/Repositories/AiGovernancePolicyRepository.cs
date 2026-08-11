using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.AiEngine.Repositories;

internal sealed class AiGovernancePolicyRepository
    : RepositoryBase<AiGovernancePolicy>, IAiGovernancePolicyRepository
{
    public AiGovernancePolicyRepository(QmsDbContext context) : base(context) { }

    public async Task<AiGovernancePolicy?> GetByTenantAsync(CancellationToken ct)
    {
        return await DbSet.FirstOrDefaultAsync(ct);
    }
}
