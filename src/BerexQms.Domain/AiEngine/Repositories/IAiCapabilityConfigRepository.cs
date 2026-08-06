using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Repositories;

public interface IAiCapabilityConfigRepository : IRepository<AiCapabilityConfig>
{
    Task<AiCapabilityConfig?> GetByCapabilityAsync(string capability, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiCapabilityConfig>> GetAllConfigsAsync(CancellationToken cancellationToken = default);
}
