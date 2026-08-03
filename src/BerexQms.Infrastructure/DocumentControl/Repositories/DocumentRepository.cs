using BerexQms.Domain.DocumentControl.Entities;
using BerexQms.Domain.DocumentControl.Repositories;
using BerexQms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BerexQms.Infrastructure.DocumentControl.Repositories;

public sealed class DocumentRepository : RepositoryBase<DocumentMaster>, IDocumentRepository
{
    public DocumentRepository(QmsDbContext context) : base(context) { }

    public async Task<DocumentMaster?> GetWithVersionsAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.Versions)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<DocumentMaster?> GetFullDetailAsync(
        Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(d => d.Versions)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<bool> DocumentNumberExistsAsync(
        string documentNumber, CancellationToken cancellationToken = default)
    {
        var normalized = documentNumber.Trim().ToUpperInvariant();
        return await DbSet.AnyAsync(
            d => d.DocumentNumber.ToUpper() == normalized, cancellationToken);
    }

    public async Task<ApprovalWorkflow?> GetApprovalWorkflowAsync(
        Guid documentVersionId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<ApprovalWorkflow>()
            .FirstOrDefaultAsync(w => w.DocumentVersionId == documentVersionId, cancellationToken);
    }

    public async Task AddApprovalWorkflowAsync(
        ApprovalWorkflow workflow, CancellationToken cancellationToken = default)
    {
        await Context.Set<ApprovalWorkflow>().AddAsync(workflow, cancellationToken);
    }

    public async Task<Distribution?> GetDistributionAsync(
        Guid distributionId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Distribution>()
            .FirstOrDefaultAsync(d => d.Id == distributionId, cancellationToken);
    }

    public async Task AddDistributionAsync(
        Distribution distribution, CancellationToken cancellationToken = default)
    {
        await Context.Set<Distribution>().AddAsync(distribution, cancellationToken);
    }

    public async Task<IReadOnlyList<Distribution>> GetDistributionsForVersionAsync(
        Guid documentVersionId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<Distribution>()
            .Where(d => d.DocumentVersionId == documentVersionId)
            .OrderByDescending(d => d.DistributedAt)
            .ToListAsync(cancellationToken);
    }
}
