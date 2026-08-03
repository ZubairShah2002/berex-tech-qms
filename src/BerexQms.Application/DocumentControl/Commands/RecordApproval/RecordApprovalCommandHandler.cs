using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.DocumentControl.Enums;
using BerexQms.Domain.DocumentControl.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.DocumentControl.Commands.RecordApproval;

internal sealed class RecordApprovalCommandHandler : ICommandHandler<RecordApprovalCommand>
{
    private readonly IDocumentRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public RecordApprovalCommandHandler(
        IDocumentRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(RecordApprovalCommand request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetWithVersionsAsync(request.DocumentId, cancellationToken);
        if (document is null)
            return Result.Failure(DocumentErrors.NotFound);

        var workflow = await _repository.GetApprovalWorkflowAsync(request.VersionId, cancellationToken);
        if (workflow is null)
            return Result.Failure(DocumentErrors.WorkflowNotFound);

        if (!Enum.TryParse<ApprovalDecision>(request.Decision, ignoreCase: true, out var decision))
            return Result.Failure(Error.Validation("Document.InvalidDecision", $"Invalid approval decision: {request.Decision}."));

        document.RecordApprovalDecision(
            request.VersionId,
            workflow,
            _currentUserService.UserId.ToString(),
            decision,
            request.Comments,
            request.Signature);

        return Result.Success();
    }
}
