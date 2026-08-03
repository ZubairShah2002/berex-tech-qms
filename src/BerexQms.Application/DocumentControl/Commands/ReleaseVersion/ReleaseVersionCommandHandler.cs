using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.DocumentControl.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.DocumentControl.Commands.ReleaseVersion;

internal sealed class ReleaseVersionCommandHandler : ICommandHandler<ReleaseVersionCommand>
{
    private readonly IDocumentRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public ReleaseVersionCommandHandler(
        IDocumentRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ReleaseVersionCommand request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetWithVersionsAsync(request.DocumentId, cancellationToken);
        if (document is null)
            return Result.Failure(DocumentErrors.NotFound);

        var workflow = await _repository.GetApprovalWorkflowAsync(request.VersionId, cancellationToken);
        if (workflow is null)
            return Result.Failure(DocumentErrors.WorkflowNotFound);

        if (!workflow.IsComplete)
            return Result.Failure(Error.Validation("Document.ApprovalIncomplete", "Approval workflow is not complete."));

        document.ReleaseVersion(request.VersionId, _currentUserService.UserId.ToString(), request.EffectiveDate);
        return Result.Success();
    }
}
