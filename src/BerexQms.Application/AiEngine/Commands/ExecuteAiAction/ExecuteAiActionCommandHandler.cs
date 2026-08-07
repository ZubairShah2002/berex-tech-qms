using System.Diagnostics;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Enums;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.ExecuteAiAction;

internal sealed class ExecuteAiActionCommandHandler
    : ICommandHandler<ExecuteAiActionCommand, ExecuteAiActionResult>
{
    private readonly IAiActionLogRepository _actionLogRepository;
    private readonly IAiPermissionService _permissionService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public ExecuteAiActionCommandHandler(
        IAiActionLogRepository actionLogRepository,
        IAiPermissionService permissionService,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _actionLogRepository = actionLogRepository;
        _permissionService = permissionService;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<ExecuteAiActionResult>> Handle(
        ExecuteAiActionCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AiActionType>(request.ActionType, true, out var actionType))
            return AiEngineErrors.InvalidActionType;

        var userLevel = await _permissionService.GetUserPermissionLevelAsync(
            _currentUserService.UserId, cancellationToken);

        if (!AiActionPolicy.IsAuthorized(userLevel, actionType))
            return AiEngineErrors.InsufficientAiPermission;

        var category = AiActionPolicy.GetCategory(actionType);
        var riskLevel = AiActionPolicy.GetRiskLevel(actionType);
        var requiresConfirmation = AiActionPolicy.RequiresConfirmation(actionType);

        var userRole = _currentUserService.Roles.FirstOrDefault() ?? "Unknown";

        var actionLog = AiActionLog.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            _currentUserService.UserId,
            userRole,
            userLevel,
            actionType,
            category,
            request.Prompt,
            riskLevel,
            requiresConfirmation);

        await _actionLogRepository.AddAsync(actionLog, cancellationToken);

        if (requiresConfirmation)
        {
            var actionSummary = BuildActionSummary(actionType, request.TargetModule, request.TargetRecordType);
            var confirmationPrompt = BuildConfirmationPrompt(actionType, riskLevel);

            var confirmationRequest = new AiConfirmationRequestDto(
                actionLog.Id,
                actionType.ToString(),
                category.ToString(),
                riskLevel.ToString(),
                actionSummary,
                request.TargetRecordId,
                IsRollbackPossible(category),
                confirmationPrompt);

            return new ExecuteAiActionResult(true, confirmationRequest, null);
        }

        // Non-confirmation actions execute immediately
        var stopwatch = Stopwatch.StartNew();

        // The actual AI action execution is a placeholder — the real implementation
        // would dispatch to module-specific Application Commands via MediatR.
        // For now, we record a successful execution.
        var reasoningSummary = $"AI action '{actionType}' executed successfully. " +
                               "I recommend reviewing the output before proceeding.";

        stopwatch.Stop();

        actionLog.RecordSuccess(
            reasoningSummary,
            request.TargetModule,
            request.TargetRecordId,
            (int)stopwatch.ElapsedMilliseconds,
            modelVersion: null,
            confidenceScore: null,
            isRollbackPossible: IsRollbackPossible(category));

        await _actionLogRepository.UpdateAsync(actionLog, cancellationToken);

        var result = MapToDto(actionLog);
        return new ExecuteAiActionResult(false, null, result);
    }

    private static string BuildActionSummary(AiActionType actionType, string? targetModule, string? targetRecordType)
    {
        var target = targetModule is not null
            ? $" on {targetModule}" + (targetRecordType is not null ? $" ({targetRecordType})" : "")
            : "";

        return $"AI will execute '{actionType}'{target}. This action requires your explicit confirmation.";
    }

    private static string BuildConfirmationPrompt(AiActionType actionType, RiskLevel riskLevel)
    {
        var riskWarning = riskLevel switch
        {
            RiskLevel.Critical => "⚠️ CRITICAL: This action may be irreversible. ",
            RiskLevel.High => "⚠️ HIGH RISK: This action affects multiple records. ",
            _ => "",
        };

        return $"{riskWarning}Do you confirm execution of '{actionType}'? " +
               "Type CONFIRM to proceed or CANCEL to abort.";
    }

    private static bool IsRollbackPossible(AiActionCategory category) =>
        category is not (AiActionCategory.Dangerous or AiActionCategory.BulkOperation);

    private static AiActionLogDto MapToDto(AiActionLog log) => new(
        log.Id,
        log.UserId,
        log.UserRole,
        log.PermissionLevel,
        log.ActionType,
        log.ActionCategory,
        log.Prompt,
        log.ReasoningSummary,
        log.AffectedModules,
        log.AffectedRecords,
        log.RiskLevel,
        log.ConfirmationStatus,
        log.RequiresConfirmation,
        log.ExecutionResult,
        log.ErrorDetail,
        log.RequestedAt,
        log.CompletedAt,
        log.DurationMs,
        log.ModelVersion,
        log.ConfidenceScore,
        log.IsRollbackPossible);
}
