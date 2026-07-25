using BerexQms.Domain.Inspection.Entities;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.Inspection.Repositories;

public sealed class InspectionRepository : RepositoryBase<InspectionRecord>, IInspectionRepository
{
    public InspectionRepository(QmsDbContext context) : base(context) { }

    public async Task<InspectionRecord?> GetWithMeasurementsAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Measurements)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<InspectionRecord?> GetWithChecklistAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Checklist)
                .ThenInclude(c => c!.Items)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<InspectionRecord?> GetFullDetailAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(r => r.Measurements)
            .Include(r => r.Checklist)
                .ThenInclude(c => c!.Items)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> InspectionNumberExistsAsync(
        string inspectionNumber, CancellationToken cancellationToken = default)
    {
        var normalized = inspectionNumber.Trim().ToUpperInvariant();
        return await DbSet.AnyAsync(r => r.InspectionNumber == normalized, cancellationToken);
    }
}
