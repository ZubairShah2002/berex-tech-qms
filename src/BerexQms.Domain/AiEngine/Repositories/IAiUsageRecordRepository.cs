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
}
