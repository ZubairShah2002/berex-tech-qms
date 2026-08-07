using System.Diagnostics;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.ConfirmWorkflow;

internal sealed class ConfirmWorkflowCommandHandler
    : ICommandHandler<ConfirmWorkflowCommand, AiWorkflowExecutionDto>
{
    private readonly IAiWorkflowExecutionRepository _executionRepository;
    private readonly IAiWorkflowDefinitionRepository _definitionRepository;

    public ConfirmWorkflowCommandHandler(
        IAiWorkflowExecutionRepository executionRepository,
        IAiWorkflowDefinitionRepository definitionRepository)
    {
        _executionRepository = executionRepository;
        _definitionRepository = definitionRepository;
    }

    public async Task<Result<AiWorkflowExecutionDto>> Handle(
        ConfirmWorkflowCommand request, CancellationToken cancellationToken)
    {
        var execution = await _executionRepository.GetByIdAsync(request.ExecutionId, cancellationToken);

        if (execution is null)
            return AiEngineErrors.WorkflowExecutionNotFound;

        if (!request.Confirm)
        {
            execution.Cancel();
            await _executionRepository.UpdateAsync(execution, cancellationToken);
            return MapToDto(execution);
        }

        execution.Confirm();

        // Execute workflow steps. In a production system, this would iterate
        // through each step defined in the AiWorkflowDefinition and dispatch
        // the corresponding Application Queries/Commands via MediatR.
        // The AI workflow orchestrator would collect results from each module
        // and aggregate them into the final output.
        var stopwatch = Stopwatch.StartNew();

        // Simulate step execution — each step represents a query to a module
        for (var step = 0; step < execution.TotalSteps; step++)
        {
            var stepResults = $"[{{\"step\":{step + 1},\"status\":\"Success\",\"module\":\"Placeholder\",\"outputSummary\":\"Data collected\"}}]";
            execution.RecordStepCompletion(stepResults);
        }

        stopwatch.Stop();

        var output = "{\"summary\":\"Workflow completed. I recommend reviewing the collected data before proceeding.\"}";
        execution.Complete(output, (int)stopwatch.ElapsedMilliseconds);

        await _executionRepository.UpdateAsync(execution, cancellationToken);
        return MapToDto(execution);
    }

    private static AiWorkflowExecutionDto MapToDto(Domain.AiEngine.Entities.AiWorkflowExecution exec) => new(
        exec.Id,
        exec.WorkflowDefinitionId,
        exec.WorkflowName,
        exec.UserId,
        exec.Status,
        exec.TotalSteps,
        exec.CompletedSteps,
        exec.FailedSteps,
        exec.Output,
        exec.StartedAt,
        exec.CompletedAt,
        exec.TotalDurationMs,
        exec.ErrorSummary);
}
