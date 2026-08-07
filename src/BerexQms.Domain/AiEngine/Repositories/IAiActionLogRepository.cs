using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Repositories;

public interface IAiActionLogRepository : IRepository<AiActionLog>
{
    Task<IReadOnlyList<AiActionLog>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiActionLog>> GetPendingConfirmationsAsync(Guid userId, CancellationToken cancellationToken = default);
}
