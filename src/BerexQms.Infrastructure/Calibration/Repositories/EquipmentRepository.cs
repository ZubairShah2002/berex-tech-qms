using BerexQms.Domain.Calibration.Entities;
using BerexQms.Domain.Calibration.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.Calibration.Repositories;

public sealed class EquipmentRepository : RepositoryBase<Equipment>, IEquipmentRepository
{
    public EquipmentRepository(QmsDbContext context) : base(context) { }

    public async Task<Equipment?> GetWithCalibrationsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Calibrations)
            .Include(e => e.Schedule)
            .Include(e => e.ImpactAssessments)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Equipment?> GetWithScheduleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Schedule)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Equipment?> GetFullDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Schedule)
            .Include(e => e.Calibrations)
            .Include(e => e.GaugeStudies)
            .Include(e => e.ImpactAssessments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await DbSet.AnyAsync(e => e.Code == normalized, cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>> GetOverdueEquipmentAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Schedule)
            .Include(e => e.Calibrations)
            .Where(e => e.Status == "Overdue" || e.Status == "DueForCalibration")
            .OrderBy(e => e.Schedule!.NextDueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<ImpactAssessment?> GetImpactAssessmentByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<ImpactAssessment>()
            .FirstOrDefaultAsync(ia => ia.Id == id, cancellationToken);
    }
}
