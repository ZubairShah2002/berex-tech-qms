using BerexQms.Domain.ProductCatalog.Entities;
using BerexQms.Domain.ProductCatalog.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.ProductCatalog.Repositories;

public sealed class PartRepository : RepositoryBase<Part>, IPartRepository
{
    public PartRepository(QmsDbContext context) : base(context) { }

    public async Task<Part?> GetByPartNumberAsync(string partNumber, CancellationToken cancellationToken = default)
    {
        var normalized = partNumber.Trim().ToUpperInvariant();
        return await DbSet
            .Include(p => p.Revisions)
            .FirstOrDefaultAsync(p => p.PartNumber == normalized, cancellationToken);
    }

    public async Task<bool> PartNumberExistsAsync(string partNumber, CancellationToken cancellationToken = default)
    {
        var normalized = partNumber.Trim().ToUpperInvariant();
        return await DbSet.AnyAsync(p => p.PartNumber == normalized, cancellationToken);
    }

    public async Task<Part?> GetWithRevisionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.Revisions)
                .ThenInclude(r => r.SpecificationParameters)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Part?> GetWithBomReferencesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(p => p.BomReferences)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
