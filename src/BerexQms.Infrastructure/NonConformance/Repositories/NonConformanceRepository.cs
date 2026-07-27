using BerexQms.Domain.NonConformance.Entities;
using BerexQms.Domain.NonConformance.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.NonConformance.Repositories;

public sealed class NonConformanceRepository : RepositoryBase<NonConformanceRecord>, INonConformanceRepository
{
    public NonConformanceRepository(QmsDbContext context) : base(context) { }

    public async Task<NonConformanceRecord?> GetWithContainmentsAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.ContainmentActions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<NonConformanceRecord?> GetWithInvestigationsAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Investigations)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<NonConformanceRecord?> GetFullDetailAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.ContainmentActions)
            .Include(r => r.Investigations)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> NcrNumberExistsAsync(
        string ncrNumber, CancellationToken cancellationToken = default)
    {
        var normalized = ncrNumber.Trim().ToUpperInvariant();
        return await DbSet.AnyAsync(r => r.NcrNumber == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<NonConformanceRecord>> FindSimilarAsync(
        Guid partId, string? defectType, Guid? supplierId,
        DateTime lookbackFrom, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Where(r => r.PartId == partId && r.CreatedAt >= lookbackFrom);

        if (!string.IsNullOrWhiteSpace(defectType))
            query = query.Where(r =>
                r.Classification != null && r.Classification.DefectType == defectType);

        if (supplierId.HasValue)
            query = query.Where(r => r.SupplierId == supplierId.Value);

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);
    }
}
