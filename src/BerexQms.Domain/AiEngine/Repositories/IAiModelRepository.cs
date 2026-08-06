using BerexQms.Domain.AiEngine.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Repositories;

public interface IAiModelRepository : IRepository<AiModel>
{
    /// <summary>
    /// Gets the currently active (champion) model for the given capability, if any.
    /// </summary>
    Task<AiModel?> GetActiveModelAsync(string capability, CancellationToken cancellationToken = default);

    Task<bool> VersionExistsAsync(string name, string version, CancellationToken cancellationToken = default);
}
