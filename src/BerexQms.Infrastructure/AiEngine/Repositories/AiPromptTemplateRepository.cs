using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.AiEngine.Repositories;

internal sealed class AiPromptTemplateRepository
    : RepositoryBase<AiPromptTemplate>, IAiPromptTemplateRepository
{
    public AiPromptTemplateRepository(QmsDbContext context) : base(context) { }

    public async Task<AiPromptTemplate?> GetActiveByTaskTypeAsync(
        string taskType, CancellationToken cancellationToken)
    {
        return await DbSet
            .Where(t => t.TaskType == taskType && t.IsActive)
            .OrderByDescending(t => t.Version)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiPromptTemplate>> GetAllActiveAsync(
        CancellationToken cancellationToken)
    {
        return await DbSet
            .Where(t => t.IsActive)
            .OrderBy(t => t.TaskType)
            .ThenByDescending(t => t.Version)
            .ToListAsync(cancellationToken);
    }
}
