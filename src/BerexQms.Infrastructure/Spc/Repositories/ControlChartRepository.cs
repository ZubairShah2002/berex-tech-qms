using BerexQms.Domain.Spc.Entities;
using BerexQms.Domain.Spc.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.Spc.Repositories;

public sealed class ControlChartRepository : RepositoryBase<ControlChart>, IControlChartRepository
{
    public ControlChartRepository(QmsDbContext context) : base(context) { }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await DbSet.AnyAsync(c => c.Code == normalized, cancellationToken);
    }

    public async Task<ControlChart?> GetWithDataPointsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(c => c.DataPoints)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ControlChart>> GetByPartIdAsync(Guid partId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.PartId == partId)
            .Include(c => c.DataPoints)
            .ToListAsync(cancellationToken);
    }
}
