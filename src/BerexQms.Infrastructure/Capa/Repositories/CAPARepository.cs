using BerexQms.Domain.Capa.Entities;
using BerexQms.Domain.Capa.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.Capa.Repositories;

public sealed class CAPARepository : RepositoryBase<CAPARecord>, ICAPARepository
{
    public CAPARepository(QmsDbContext context) : base(context) { }

    public async Task<CAPARecord?> GetWithActionsAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Actions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<CAPARecord?> GetWithVerificationsAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Verifications)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<CAPARecord?> GetFullDetailAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.RootCauseAnalysis)
            .Include(r => r.Actions)
            .Include(r => r.Verifications)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> CapaNumberExistsAsync(
        string capaNumber, CancellationToken cancellationToken = default)
    {
        var normalized = capaNumber.Trim().ToUpperInvariant();
        return await DbSet.AnyAsync(r => r.CapaNumber == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<CAPARecord>> GetByNonConformanceIdAsync(
        Guid nonConformanceId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(r => r.SourceNonConformanceId == nonConformanceId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
