using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.ExecuteWorkflow;

internal sealed class ExecuteWorkflowCommandHandler
    : ICommandHandler<ExecuteWorkflowCommand, AiWorkflowExecutionDto>
{
    private readonly IAiWorkflowDefinitionRepository _definitionRepository;
    private readonly IAiWorkflowExecutionRepository _executionRepository;
    private readonly IAiPermissionService _permissionService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public ExecuteWorkflowCommandHandler(
        IAiWorkflowDefinitionRepository definitionRepository,
        IAiWorkflowExecutionRepository executionRepository,
        IAiPermissionService permissionService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _definitionRepository = definitionRepository;
        _executionRepository = executionRepository;
        _permissionService = permissionService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AiWorkflowExecutionDto>> Handle(
        ExecuteWorkflowCommand request, CancellationToken cancellationToken)
    {
        var definition = await _definitionRepository.GetByIdAsync(
            request.WorkflowDefinitionId, cancellationToken);

        if (definition is null)
            return AiEngineErrors.WorkflowDefinitionNotFound;

        if (!definition.IsActive)
            return AiEngineErrors.WorkflowDefinitionInactive;

        // Check user has required permission level
        if (!Enum.TryParse<AiPermissionLevel>(definition.MinimumPermissionLevel, true, out var requiredLevel))
            return AiEngineErrors.InvalidPermissionLevel;

        var userLevel = await _permissionService.GetUserPermissionLevelAsync(
            _currentUserService.UserId, cancellationToken);

        if ((int)userLevel < (int)requiredLevel)
            return AiEngineErrors.InsufficientAiPermission;

        // Count steps from definition (JSON array — count top-level entries)
        var stepCount = CountSteps(definition.StepsDefinition);

        var execution = AiWorkflowExecution.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            definition.Id,
            definition.Name,
            _currentUserService.UserId,
            stepCount);

        await _executionRepository.AddAsync(execution, cancellationToken);

        return MapToDto(execution);
    }

    private static int CountSteps(string stepsJson)
    {
        // Simple count: count occurrences of "stepName" in the JSON array
        var count = 0;
        var index = 0;
        while ((index = stepsJson.IndexOf("\"stepName\"", index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index++;
        }
        return Math.Max(count, 1);
    }

    private static AiWorkflowExecutionDto MapToDto(AiWorkflowExecution exec) => new(
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
