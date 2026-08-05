using BerexQms.Domain.Training.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Training.Repositories;

public interface ITrainingAssignmentRepository : IRepository<TrainingAssignment>
{
    Task<TrainingAssignment?> GetWithCompletionAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TrainingAssignment>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default);
}
