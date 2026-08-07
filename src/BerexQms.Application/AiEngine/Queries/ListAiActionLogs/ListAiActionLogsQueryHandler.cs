using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.ListAiActionLogs;

internal sealed class ListAiActionLogsQueryHandler
    : IQueryHandler<ListAiActionLogsQuery, PagedResult<AiActionLogDto>>
{
    private readonly IAiActionLogRepository _repository;

    public ListAiActionLogsQueryHandler(IAiActionLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<AiActionLogDto>>> Handle(
        ListAiActionLogsQuery request, CancellationToken cancellationToken)
    {
        var spec = new AiActionLogFilterSpec(
            request.Page, request.PageSize, request.ActionType,
            request.PermissionLevel, request.ExecutionResult, request.UserId);

        var countSpec = new AiActionLogFilterSpec(
            1, int.MaxValue, request.ActionType,
            request.PermissionLevel, request.ExecutionResult, request.UserId);

        var items = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

        var dtos = items.Select(MapToDto).ToList();
        return Result.Success<PagedResult<AiActionLogDto>>(
            new PagedResult<AiActionLogDto>(dtos, totalCount, request.Page, request.PageSize));
    }

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

    private sealed class AiActionLogFilterSpec : Specification<AiActionLog>
    {
        public AiActionLogFilterSpec(
            int page, int pageSize,
            string? actionType, string? permissionLevel,
            string? executionResult, Guid? userId)
        {
            ApplyCriteria(log =>
                (actionType == null || log.ActionType == actionType) &&
                (permissionLevel == null || log.PermissionLevel == permissionLevel) &&
                (executionResult == null || log.ExecutionResult == executionResult) &&
                (!userId.HasValue || log.UserId == userId.Value));

            ApplyOrderByDescending(log => log.RequestedAt);

            if (pageSize < int.MaxValue)
                ApplyPaging((page - 1) * pageSize, pageSize);
        }
    }
}
