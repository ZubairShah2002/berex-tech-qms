using BerexQms.Domain.Identity.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Identity.Repositories;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
}
