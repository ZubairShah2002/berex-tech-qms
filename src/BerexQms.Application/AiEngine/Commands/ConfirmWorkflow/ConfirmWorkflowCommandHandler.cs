using System.Diagnostics;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.ConfirmWorkflow;

internal sealed class ConfirmWorkflowCommandHandler
    : ICommandHandler<ConfirmWorkflowCommand, AiWorkflowExecutionDto>
{
    private readonly IAiWorkflowExecutionRepository _executionRepository;
    private readonly IAiWorkflowDefinitionRepository _definitionRepository;
    private readonly IAiPermissionService _permissionService;
    private readonly ICurrentUserService _currentUserService;

    public ConfirmWorkflowCommandHandler(
        IAiWorkflowExecutionRepository executionRepository,
        IAiWorkflowDefinitionRepository definitionRepository,
        IAiPermissionService permissionService,
        ICurrentUserService currentUserService)
    {
        _executionRepository = executionRepository;
        _definitionRepository = definitionRepository;
        _permissionService = permissionService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AiWorkflowExecutionDto>> Handle(
        ConfirmWorkflowCommand request, CancellationToken cancellationToken)
    {
        var execution = await _executionRepository.GetByIdAsync(request.ExecutionId, cancellationToken);

        if (execution is null)
            return AiEngineErrors.WorkflowExecutionNotFound;

        // Verify the confirming user has the required permission level for this workflow
        var definition = await _definitionRepository.GetByIdAsync(
            execution.WorkflowDefinitionId, cancellationToken);

        if (definition is not null &&
            Enum.TryParse<AiPermissionLevel>(definition.MinimumPermissionLevel, true, out var requiredLevel))
        {
            var userLevel = await _permissionService.GetUserPermissionLevelAsync(
                _currentUserService.UserId, cancellationToken);

            if ((int)userLevel < (int)requiredLevel)
                return AiEngineErrors.InsufficientAiPermission;
        }

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
