using BerexQms.Domain.Inspection.Entities;
using BerexQms.Domain.Inspection.Enums;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Inspection.Repositories;

public interface ISamplingPlanRepository : IRepository<SamplingPlan>
{
    Task<SamplingPlan?> GetActiveForPartAsync(Guid partId, InspectionType inspectionType, CancellationToken cancellationToken = default);
    Task<SamplingPlan?> GetActiveForPartAndSupplierAsync(Guid partId, Guid supplierId, InspectionType inspectionType, CancellationToken cancellationToken = default);
}
