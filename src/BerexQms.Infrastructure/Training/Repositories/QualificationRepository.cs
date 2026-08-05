using BerexQms.Domain.Training.Entities;
using BerexQms.Domain.Training.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.Training.Repositories;

public sealed class QualificationRepository : RepositoryBase<Qualification>, IQualificationRepository
{
    public QualificationRepository(QmsDbContext context) : base(context) { }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await DbSet.AnyAsync(q => q.Code == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Qualification>> GetByIdsAsync(
        IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
            return Array.Empty<Qualification>();

        return await DbSet.Where(q => idList.Contains(q.Id)).ToListAsync(cancellationToken);
    }
}
