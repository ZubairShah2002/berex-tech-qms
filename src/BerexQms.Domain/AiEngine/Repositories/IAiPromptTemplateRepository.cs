using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Repositories;

public interface IAiPromptTemplateRepository : IRepository<AiPromptTemplate>
{
    Task<AiPromptTemplate?> GetActiveByTaskTypeAsync(
        string taskType, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiPromptTemplate>> GetAllActiveAsync(
        CancellationToken cancellationToken = default);
}
