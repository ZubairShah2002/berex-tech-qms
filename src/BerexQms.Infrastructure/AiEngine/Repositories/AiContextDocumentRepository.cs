using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.AiEngine.Repositories;

public sealed class AiContextDocumentRepository : RepositoryBase<AiContextDocument>, IAiContextDocumentRepository
{
    public AiContextDocumentRepository(QmsDbContext context) : base(context) { }

    public async Task<IReadOnlyList<AiContextDocument>> GetByModuleAsync(
        string sourceModule, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(d => d.SourceModule == sourceModule)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiContextDocument>> GetByContextTypeAsync(
        string contextType, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(d => d.ContextType == contextType)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<AiContextDocument?> GetBySourceEntityAsync(
        string sourceModule, string sourceEntityId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(
                d => d.SourceModule == sourceModule && d.SourceEntityId == sourceEntityId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<AiContextDocument>> GetPendingIndexingAsync(
        CancellationToken cancellationToken = default)
    {
        var pending = AiEmbeddingStatus.Pending.ToString();
        var stale = AiEmbeddingStatus.Stale.ToString();
        var failed = AiEmbeddingStatus.Failed.ToString();

        return await DbSet
            .Where(d => d.EmbeddingStatus == pending
                        || d.EmbeddingStatus == stale
                        || d.EmbeddingStatus == failed)
            .OrderBy(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AiContextDocument>> SearchByContentAsync(
        string searchTerm, CancellationToken cancellationToken = default)
    {
        var term = searchTerm.ToLower();

        return await DbSet
            .Where(d => EF.Functions.ILike(d.Title, $"%{term}%")
                        || EF.Functions.ILike(d.Content, $"%{term}%"))
            .OrderByDescending(d => d.IndexedAt ?? d.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
