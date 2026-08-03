using BerexQms.Domain.DocumentControl.Entities;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.DocumentControl.Repositories;

public interface IDocumentRepository : IRepository<DocumentMaster>
{
    Task<DocumentMaster?> GetWithVersionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DocumentMaster?> GetFullDetailAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DocumentNumberExistsAsync(string documentNumber, CancellationToken cancellationToken = default);
    Task<ApprovalWorkflow?> GetApprovalWorkflowAsync(Guid documentVersionId, CancellationToken cancellationToken = default);
    Task AddApprovalWorkflowAsync(ApprovalWorkflow workflow, CancellationToken cancellationToken = default);
    Task<Distribution?> GetDistributionAsync(Guid distributionId, CancellationToken cancellationToken = default);
    Task AddDistributionAsync(Distribution distribution, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Distribution>> GetDistributionsForVersionAsync(Guid documentVersionId, CancellationToken cancellationToken = default);
}
