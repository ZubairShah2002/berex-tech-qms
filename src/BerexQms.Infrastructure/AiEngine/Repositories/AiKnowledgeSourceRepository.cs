using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.AiEngine.Repositories;

public sealed class AiKnowledgeSourceRepository : RepositoryBase<AiKnowledgeSource>, IAiKnowledgeSourceRepository
{
    public AiKnowledgeSourceRepository(QmsDbContext context) : base(context) { }

    public async Task<AiKnowledgeSource?> GetByModuleAsync(
        string module, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(s => s.Module == module, cancellationToken);
    }

    public async Task<IReadOnlyList<AiKnowledgeSource>> GetActiveSourcesAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }
}
