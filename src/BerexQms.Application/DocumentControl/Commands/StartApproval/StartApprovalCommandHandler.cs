using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.DocumentControl.DTOs;
using BerexQms.Domain.DocumentControl.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.DocumentControl.Commands.StartApproval;

internal sealed class StartApprovalCommandHandler : ICommandHandler<StartApprovalCommand, ApprovalWorkflowDto>
{
    private readonly IDocumentRepository _repository;

    public StartApprovalCommandHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ApprovalWorkflowDto>> Handle(StartApprovalCommand request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetWithVersionsAsync(request.DocumentId, cancellationToken);
        if (document is null)
            return DocumentErrors.NotFound;

        var workflow = document.StartApproval(request.VersionId, request.ApproverIds);
        await _repository.AddApprovalWorkflowAsync(workflow, cancellationToken);

        return new ApprovalWorkflowDto(
            workflow.Id,
            workflow.DocumentVersionId,
            workflow.CurrentStepOrder,
            workflow.IsComplete,
            workflow.IsRejected,
            workflow.Steps.Select(s => new ApprovalStepDto(
                s.StepOrder, s.ApproverId, s.Decision?.ToString(), s.Comments, s.DecidedAt)).ToList(),
            workflow.CreatedAt,
            workflow.CompletedAt);
    }
}
