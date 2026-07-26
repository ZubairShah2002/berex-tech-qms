using BerexQms.Domain.Inspection.Entities;
using BerexQms.Domain.Inspection.Enums;
using BerexQms.Domain.Inspection.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.Inspection.Repositories;

public sealed class SamplingPlanRepository : RepositoryBase<SamplingPlan>, ISamplingPlanRepository
{
    public SamplingPlanRepository(QmsDbContext context) : base(context) { }

    public async Task<SamplingPlan?> GetActiveForPartAsync(
        Guid partId, InspectionType inspectionType, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(
            s => s.PartId == partId
                 && s.InspectionType == inspectionType
                 && s.IsActive
                 && s.SupplierId == null,
            cancellationToken);
    }

    public async Task<SamplingPlan?> GetActiveForPartAndSupplierAsync(
        Guid partId, Guid supplierId, InspectionType inspectionType,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(
            s => s.PartId == partId
                 && s.SupplierId == supplierId
                 && s.InspectionType == inspectionType
                 && s.IsActive,
            cancellationToken);
    }
}
