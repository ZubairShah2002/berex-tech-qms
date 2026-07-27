using BerexQms.Domain.NonConformance.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.NonConformance.Repositories;

public interface INonConformanceRepository : IRepository<NonConformanceRecord>
{
    Task<NonConformanceRecord?> GetWithContainmentsAsync(
        Guid id, CancellationToken cancellationToken = default);

    Task<NonConformanceRecord?> GetWithInvestigationsAsync(
        Guid id, CancellationToken cancellationToken = default);

    Task<NonConformanceRecord?> GetFullDetailAsync(
        Guid id, CancellationToken cancellationToken = default);

    Task<bool> NcrNumberExistsAsync(
        string ncrNumber, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NonConformanceRecord>> FindSimilarAsync(
        Guid partId, string? defectType, Guid? supplierId,
        DateTime lookbackFrom, CancellationToken cancellationToken = default);
}
