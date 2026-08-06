using BerexQms.Domain.Spc.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Spc.Repositories;

public interface IControlChartRepository : IRepository<ControlChart>
{
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<ControlChart?> GetWithDataPointsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ControlChart>> GetByPartIdAsync(Guid partId, CancellationToken cancellationToken = default);
}
