using BerexQms.Domain.DocumentControl.Enums;
using BerexQms.Domain.DocumentControl.ValueObjects;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.DocumentControl.Entities;

public sealed class ApprovalWorkflow : Entity<Guid>
{
    private readonly List<ApprovalStep> _steps = [];

    public Guid DocumentVersionId { get; private set; }
    public int CurrentStepOrder { get; private set; }
    public bool IsComplete { get; private set; }
    public bool IsRejected { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public IReadOnlyList<ApprovalStep> Steps => _steps.AsReadOnly();

    private ApprovalWorkflow() { }

    internal static ApprovalWorkflow Create(
        Guid id,
        TenantId tenantId,
        Guid documentVersionId,
        IReadOnlyList<string> approverIds)
    {
        if (approverIds.Count == 0)
            throw new DomainException("At least one approver is required.");

        var workflow = new ApprovalWorkflow
        {
            Id = id,
            TenantId = tenantId,
            DocumentVersionId = documentVersionId,
            CurrentStepOrder = 1,
            IsComplete = false,
            IsRejected = false,
            CreatedAt = DateTime.UtcNow,
        };

        for (var i = 0; i < approverIds.Count; i++)
        {
            workflow._steps.Add(new ApprovalStep(
                StepOrder: i + 1,
                ApproverId: approverIds[i],
                Decision: null,
                Comments: null,
                Signature: null,
                DecidedAt: null));
        }

        return workflow;
    }

    internal void RecordDecision(string approverId, ApprovalDecision decision, string? comments, string? signature)
    {
        if (IsComplete || IsRejected)
            throw new DomainException("Workflow is already completed.");

        var currentStep = _steps.FirstOrDefault(s => s.StepOrder == CurrentStepOrder)
            ?? throw new DomainException("No pending approval step found.");

        if (currentStep.ApproverId != approverId)
            throw new DomainException("Only the designated approver can record a decision.");

        var updatedStep = currentStep with
        {
            Decision = decision,
            Comments = comments,
            Signature = signature,
            DecidedAt = DateTime.UtcNow,
        };

        var index = _steps.FindIndex(s => s.StepOrder == CurrentStepOrder);
        _steps[index] = updatedStep;

        if (decision == ApprovalDecision.Rejected)
        {
            IsRejected = true;
            CompletedAt = DateTime.UtcNow;
            return;
        }

        if (CurrentStepOrder >= _steps.Count)
        {
            IsComplete = true;
            CompletedAt = DateTime.UtcNow;
        }
        else
        {
            CurrentStepOrder++;
        }
    }
}
