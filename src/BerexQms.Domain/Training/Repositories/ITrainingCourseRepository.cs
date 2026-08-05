using BerexQms.Domain.Training.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Training.Repositories;

public interface ITrainingCourseRepository : IRepository<TrainingCourse>
{
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<TrainingCourse?> GetWithQualificationAsync(Guid id, CancellationToken cancellationToken = default);
}
