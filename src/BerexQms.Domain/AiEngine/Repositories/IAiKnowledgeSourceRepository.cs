using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Repositories;

public interface IAiKnowledgeSourceRepository : IRepository<AiKnowledgeSource>
{
    Task<AiKnowledgeSource?> GetByModuleAsync(
        string module, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiKnowledgeSource>> GetActiveSourcesAsync(
        CancellationToken cancellationToken = default);
}
