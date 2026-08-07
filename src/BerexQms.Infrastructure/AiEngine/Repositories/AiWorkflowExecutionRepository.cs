using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.AiEngine.Repositories;

public sealed class AiWorkflowExecutionRepository : RepositoryBase<AiWorkflowExecution>, IAiWorkflowExecutionRepository
{
    public AiWorkflowExecutionRepository(QmsDbContext context) : base(context) { }

    public async Task<IReadOnlyList<AiWorkflowExecution>> GetByUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.StartedAt)
            .ToListAsync(cancellationToken);
    }
}
