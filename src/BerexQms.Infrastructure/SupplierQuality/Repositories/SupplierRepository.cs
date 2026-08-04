using BerexQms.Domain.SupplierQuality.Entities;
using BerexQms.Domain.SupplierQuality.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.SupplierQuality.Repositories;

public sealed class SupplierRepository : RepositoryBase<Supplier>, ISupplierRepository
{
    public SupplierRepository(QmsDbContext context) : base(context) { }

    public async Task<Supplier?> GetWithApprovalsAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Approvals)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Supplier?> GetWithScarsAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Scars)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Supplier?> GetFullDetailAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.Approvals)
            .Include(s => s.Scorecards)
            .Include(s => s.Scars)
            .Include(s => s.ApprovedParts)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(
        string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await DbSet.AnyAsync(s => s.Code == normalized, cancellationToken);
    }

    public async Task<SCARRecord?> GetScarByIdAsync(
        Guid scarId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<SCARRecord>()
            .FirstOrDefaultAsync(s => s.Id == scarId, cancellationToken);
    }
}
