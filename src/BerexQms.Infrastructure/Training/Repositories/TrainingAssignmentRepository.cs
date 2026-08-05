using BerexQms.Domain.Training.Entities;
using BerexQms.Domain.Training.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.Training.Repositories;

public sealed class TrainingAssignmentRepository : RepositoryBase<TrainingAssignment>, ITrainingAssignmentRepository
{
    public TrainingAssignmentRepository(QmsDbContext context) : base(context) { }

    public async Task<TrainingAssignment?> GetWithCompletionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TrainingAssignment>> GetByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.AssignedDate)
            .ToListAsync(cancellationToken);
    }
}
