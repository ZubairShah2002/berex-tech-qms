using BerexQms.Domain.Identity.Entities;
using BerexQms.Domain.Identity.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.Identity.Repositories;

public sealed class RoleRepository : RepositoryBase<Role>, IRoleRepository
{
    public RoleRepository(QmsDbContext context) : base(context) { }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await DbSet.Where(r => idList.Contains(r.Id)).ToListAsync(cancellationToken);
    }
}
