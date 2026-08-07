using BerexQms.Domain.AiEngine.Enums;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Exceptions;
using BerexQms.SharedKernel.ValueObjects;

namespace BerexQms.Domain.AiEngine.Entities;

/// <summary>
/// Records a single execution of an <see cref="AiWorkflowDefinition"/>.
/// Tracks each step's result and the overall execution outcome.
/// </summary>
public sealed class AiWorkflowExecution : AggregateRoot<Guid>, IAuditableEntity
{
    public Guid WorkflowDefinitionId { get; private set; }
    public string WorkflowName { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public int TotalSteps { get; private set; }
    public int CompletedSteps { get; private set; }
    public int FailedSteps { get; private set; }

    /// <summary>
    /// JSON array recording each step's execution result:
    /// stepName, module, status (Success/Failed/Skipped), outputSummary, durationMs, error.
    /// </summary>
    public string? StepResults { get; private set; }

    /// <summary>
    /// JSON containing the aggregated output of the workflow execution
    /// (e.g. the final management review content).
    /// </summary>
    public string? Output { get; private set; }

    public DateTime StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int? TotalDurationMs { get; private set; }
    public string? ErrorSummary { get; private set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    private AiWorkflowExecution() { }

    public static AiWorkflowExecution Create(
        Guid id,
        TenantId tenantId,
        Guid workflowDefinitionId,
        string workflowName,
        Guid userId,
        int totalSteps)
    {
        if (workflowDefinitionId == Guid.Empty)
            throw new DomainException("Workflow definition ID is required.");
        if (userId == Guid.Empty)
            throw new DomainException("User ID is required.");
        if (totalSteps <= 0)
            throw new DomainException("Total steps must be greater than zero.");

        return new AiWorkflowExecution
        {
            Id = id,
            TenantId = tenantId,
            WorkflowDefinitionId = workflowDefinitionId,
            WorkflowName = workflowName,
            UserId = userId,
            Status = AiWorkflowStatus.PendingConfirmation.ToString(),
            TotalSteps = totalSteps,
            CompletedSteps = 0,
            FailedSteps = 0,
            StartedAt = DateTime.UtcNow,
        };
    }

    public void Confirm()
    {
        if (Status != AiWorkflowStatus.PendingConfirmation.ToString())
            throw new DomainException("Only a pending workflow can be confirmed.");

        Status = AiWorkflowStatus.Running.ToString();
    }

    public void RecordStepCompletion(string stepResults)
    {
        if (Status != AiWorkflowStatus.Running.ToString())
            throw new DomainException("Steps can only be recorded on a running workflow.");

        CompletedSteps++;
        StepResults = stepResults;
    }

    public void RecordStepFailure(string stepResults)
    {
        if (Status != AiWorkflowStatus.Running.ToString())
            throw new DomainException("Step failures can only be recorded on a running workflow.");

        FailedSteps++;
        StepResults = stepResults;
    }

    public void Complete(string? output, int totalDurationMs)
    {
        Status = FailedSteps > 0
            ? AiWorkflowStatus.Failed.ToString()
            : AiWorkflowStatus.Completed.ToString();

        Output = output;
        CompletedAt = DateTime.UtcNow;
        TotalDurationMs = totalDurationMs;
    }

    public void Fail(string errorSummary, int totalDurationMs)
    {
        Status = AiWorkflowStatus.Failed.ToString();
        ErrorSummary = errorSummary;
        CompletedAt = DateTime.UtcNow;
        TotalDurationMs = totalDurationMs;
    }

    public void Cancel()
    {
        if (Status is not ("PendingConfirmation" or "Running"))
            throw new DomainException("Only a pending or running workflow can be cancelled.");

        Status = AiWorkflowStatus.Cancelled.ToString();
        CompletedAt = DateTime.UtcNow;
    }
}
