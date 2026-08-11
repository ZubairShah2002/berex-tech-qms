using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Repositories;

public interface IAiUserPreferenceRepository : IRepository<AiUserPreference>
{
    /// <summary>Get the AI preference for a specific user within the current tenant.</summary>
    Task<AiUserPreference?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
}
