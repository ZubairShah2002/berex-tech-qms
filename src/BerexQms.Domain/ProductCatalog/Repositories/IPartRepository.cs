using BerexQms.Domain.ProductCatalog.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.ProductCatalog.Repositories;

public interface IPartRepository : IRepository<Part>
{
    Task<Part?> GetByPartNumberAsync(string partNumber, CancellationToken cancellationToken = default);
    Task<bool> PartNumberExistsAsync(string partNumber, CancellationToken cancellationToken = default);
    Task<Part?> GetWithRevisionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Part?> GetWithBomReferencesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Part?> GetFullDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Part>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
