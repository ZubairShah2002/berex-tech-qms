using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Repositories;

public interface IAiContextDocumentRepository : IRepository<AiContextDocument>
{
    Task<IReadOnlyList<AiContextDocument>> GetByModuleAsync(
        string sourceModule, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiContextDocument>> GetByContextTypeAsync(
        string contextType, CancellationToken cancellationToken = default);

    Task<AiContextDocument?> GetBySourceEntityAsync(
        string sourceModule, string sourceEntityId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiContextDocument>> GetPendingIndexingAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AiContextDocument>> SearchByContentAsync(
        string searchTerm, CancellationToken cancellationToken = default);
}
