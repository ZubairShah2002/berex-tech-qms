using BerexQms.Domain.Training.Entities;
using BerexQms.Domain.Training.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.Training.Repositories;

public sealed class TrainingCourseRepository : RepositoryBase<TrainingCourse>, ITrainingCourseRepository
{
    public TrainingCourseRepository(QmsDbContext context) : base(context) { }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToUpperInvariant();
        return await DbSet.AnyAsync(c => c.Code == normalized, cancellationToken);
    }

    public async Task<TrainingCourse?> GetWithQualificationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
