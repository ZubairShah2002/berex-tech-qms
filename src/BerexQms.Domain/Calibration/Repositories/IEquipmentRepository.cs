using BerexQms.Domain.Calibration.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Calibration.Repositories;

public interface IEquipmentRepository : IRepository<Equipment>
{
    Task<Equipment?> GetWithCalibrationsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Equipment?> GetWithScheduleAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Equipment?> GetFullDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Equipment>> GetOverdueEquipmentAsync(CancellationToken cancellationToken = default);
    Task<ImpactAssessment?> GetImpactAssessmentByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
