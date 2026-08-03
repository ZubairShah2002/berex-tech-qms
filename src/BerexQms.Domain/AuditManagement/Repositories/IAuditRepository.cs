using BerexQms.Domain.AuditManagement.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AuditManagement.Repositories;

public interface IAuditRepository : IRepository<AuditPlan>
{
    Task<AuditPlan?> GetWithAuditsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AuditPlan?> GetFullDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AuditRecord?> GetAuditRecordAsync(Guid auditRecordId, CancellationToken cancellationToken = default);
    Task<bool> PlanNameExistsAsync(string name, int year, CancellationToken cancellationToken = default);
}
