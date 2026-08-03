using BerexQms.Domain.AuditManagement.Entities;
using BerexQms.Domain.AuditManagement.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.AuditManagement.Repositories;

public sealed class AuditRepository : RepositoryBase<AuditPlan>, IAuditRepository
{
    public AuditRepository(QmsDbContext context) : base(context) { }

    public async Task<AuditPlan?> GetWithAuditsAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Audits)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<AuditPlan?> GetFullDetailAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Audits)
                .ThenInclude(r => r.Findings)
            .Include(a => a.Audits)
                .ThenInclude(r => r.Checklists)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<AuditRecord?> GetAuditRecordAsync(
        Guid auditRecordId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<AuditRecord>()
            .Include(r => r.Findings)
            .Include(r => r.Checklists)
            .FirstOrDefaultAsync(r => r.Id == auditRecordId, cancellationToken);
    }

    public async Task<bool> PlanNameExistsAsync(
        string name, int year, CancellationToken cancellationToken = default)
    {
        var normalized = name.Trim().ToUpperInvariant();
        return await DbSet.AnyAsync(
            a => a.PlanName.ToUpper() == normalized && a.Year == year, cancellationToken);
    }
}
