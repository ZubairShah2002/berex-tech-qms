using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Repositories;

public interface IAiGovernancePolicyRepository : IRepository<AiGovernancePolicy>
{
    /// <summary>Get the governance policy for the current tenant.</summary>
    Task<AiGovernancePolicy?> GetByTenantAsync(CancellationToken ct = default);
}
