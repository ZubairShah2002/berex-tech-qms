using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Repositories;

public interface IAiInteractionRepository : IRepository<AiInteraction>
{
    Task<IReadOnlyList<AiInteraction>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiInteraction>> GetByCapabilityAsync(string capability, CancellationToken cancellationToken = default);
}
