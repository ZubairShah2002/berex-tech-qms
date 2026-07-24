using BerexQms.Domain.Identity.Entities;
using BerexQms.Domain.Identity.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.Identity.Repositories;

public sealed class TenantRepository : RepositoryBase<Tenant>, ITenantRepository
{
    public TenantRepository(QmsDbContext context) : base(context) { }

    public async Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await DbSet.FirstOrDefaultAsync(t => t.Code == normalizedCode, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await DbSet.AnyAsync(t => t.Code == normalizedCode, cancellationToken);
    }
}
