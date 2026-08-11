using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Repositories;

public interface IAiUsageRecordRepository : IRepository<AiUsageRecord>
{
    Task<IReadOnlyList<AiUsageRecord>> GetByProviderAsync(
        string provider, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiUsageRecord>> GetByTaskTypeAsync(
        string taskType, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiUsageRecord>> GetRecentAsync(
        int count, CancellationToken cancellationToken = default);

    /// <summary>Count AI requests by a specific user today (UTC).</summary>
    Task<int> CountUserRequestsTodayAsync(
        Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Count AI requests for the tenant this calendar month (UTC).</summary>
    Task<int> CountTenantRequestsThisMonthAsync(
        Guid tenantId, CancellationToken cancellationToken = default);
}
