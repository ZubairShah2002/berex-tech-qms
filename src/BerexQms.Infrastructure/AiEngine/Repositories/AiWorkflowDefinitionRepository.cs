using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.AiEngine.Repositories;

public sealed class AiWorkflowDefinitionRepository : RepositoryBase<AiWorkflowDefinition>, IAiWorkflowDefinitionRepository
{
    public AiWorkflowDefinitionRepository(QmsDbContext context) : base(context) { }

    public async Task<AiWorkflowDefinition?> GetByNameAsync(
        string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<AiWorkflowDefinition>> GetActiveWorkflowsAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}
