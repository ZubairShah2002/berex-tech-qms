using BerexQms.Domain.SupplierQuality.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.SupplierQuality.Repositories;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<Supplier?> GetWithApprovalsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Supplier?> GetWithScarsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Supplier?> GetFullDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<SCARRecord?> GetScarByIdAsync(Guid scarId, CancellationToken cancellationToken = default);
}
