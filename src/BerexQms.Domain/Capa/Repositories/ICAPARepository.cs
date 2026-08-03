using BerexQms.Domain.Capa.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Capa.Repositories;

public interface ICAPARepository : IRepository<CAPARecord>
{
    Task<CAPARecord?> GetWithActionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CAPARecord?> GetWithVerificationsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CAPARecord?> GetFullDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CapaNumberExistsAsync(string capaNumber, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CAPARecord>> GetByNonConformanceIdAsync(Guid nonConformanceId, CancellationToken cancellationToken = default);
}
