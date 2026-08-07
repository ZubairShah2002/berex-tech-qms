using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Repositories;

public interface IAiWorkflowDefinitionRepository : IRepository<AiWorkflowDefinition>
{
    Task<AiWorkflowDefinition?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AiWorkflowDefinition>> GetActiveWorkflowsAsync(CancellationToken cancellationToken = default);
}
