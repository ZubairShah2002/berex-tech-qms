using BerexQms.Domain.Inspection.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Inspection.Repositories;

public interface IInspectionRepository : IRepository<InspectionRecord>
{
    Task<InspectionRecord?> GetWithMeasurementsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InspectionRecord?> GetWithChecklistAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InspectionRecord?> GetFullDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> InspectionNumberExistsAsync(string inspectionNumber, CancellationToken cancellationToken = default);
}
