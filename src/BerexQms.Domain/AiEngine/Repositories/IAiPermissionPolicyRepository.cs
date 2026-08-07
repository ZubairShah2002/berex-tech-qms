using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Repositories;

public interface IAiPermissionPolicyRepository : IRepository<AiPermissionPolicy>
{
    Task<AiPermissionPolicy?> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
