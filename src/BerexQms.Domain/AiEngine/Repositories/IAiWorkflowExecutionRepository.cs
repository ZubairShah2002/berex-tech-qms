using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Repositories;

public interface IAiWorkflowExecutionRepository : IRepository<AiWorkflowExecution>
{
    Task<IReadOnlyList<AiWorkflowExecution>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
