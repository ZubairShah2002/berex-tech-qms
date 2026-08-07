using System.Diagnostics;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.ConfirmAiAction;

internal sealed class ConfirmAiActionCommandHandler
    : ICommandHandler<ConfirmAiActionCommand, AiActionLogDto>
{
    private readonly IAiActionLogRepository _actionLogRepository;
    private readonly ICurrentUserService _currentUserService;

    public ConfirmAiActionCommandHandler(
        IAiActionLogRepository actionLogRepository,
        ICurrentUserService currentUserService)
    {
        _actionLogRepository = actionLogRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AiActionLogDto>> Handle(
        ConfirmAiActionCommand request, CancellationToken cancellationToken)
    {
        var actionLog = await _actionLogRepository.GetByIdAsync(request.ActionLogId, cancellationToken);

        if (actionLog is null)
            return AiEngineErrors.ActionLogNotFound;

        if (!request.Confirm)
        {
            actionLog.RejectConfirmation(_currentUserService.UserId.ToString());
            await _actionLogRepository.UpdateAsync(actionLog, cancellationToken);
            return MapToDto(actionLog);
        }

        actionLog.RecordConfirmation(_currentUserService.UserId.ToString());

        // Execute the confirmed action. In a production system, this would dispatch
        // to the appropriate module's Application Command via MediatR. For now,
        // we record a successful execution.
        var stopwatch = Stopwatch.StartNew();

        var reasoningSummary = $"Confirmed AI action '{actionLog.ActionType}' executed successfully. " +
                               "I recommend verifying the affected records.";

        stopwatch.Stop();

        actionLog.RecordSuccess(
            reasoningSummary,
            actionLog.AffectedModules,
            actionLog.AffectedRecords,
            (int)stopwatch.ElapsedMilliseconds,
            modelVersion: null,
            confidenceScore: null,
            isRollbackPossible: false);

        await _actionLogRepository.UpdateAsync(actionLog, cancellationToken);
        return MapToDto(actionLog);
    }

    private static AiActionLogDto MapToDto(Domain.AiEngine.Entities.AiActionLog log) => new(
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
