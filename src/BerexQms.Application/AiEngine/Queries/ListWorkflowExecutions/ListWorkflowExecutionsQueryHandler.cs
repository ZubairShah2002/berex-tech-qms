using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Abstractions;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.ListWorkflowExecutions;

internal sealed class ListWorkflowExecutionsQueryHandler
    : IQueryHandler<ListWorkflowExecutionsQuery, PagedResult<AiWorkflowExecutionDto>>
{
    private readonly IAiWorkflowExecutionRepository _repository;

    public ListWorkflowExecutionsQueryHandler(IAiWorkflowExecutionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<AiWorkflowExecutionDto>>> Handle(
        ListWorkflowExecutionsQuery request, CancellationToken cancellationToken)
    {
        var spec = new WorkflowExecutionFilterSpec(
            request.Page, request.PageSize, request.Status, request.UserId);

        var countSpec = new WorkflowExecutionFilterSpec(
            1, int.MaxValue, request.Status, request.UserId);

        var items = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(countSpec, cancellationToken);

        var dtos = items.Select(MapToDto).ToList();
        return Result.Success<PagedResult<AiWorkflowExecutionDto>>(
            new PagedResult<AiWorkflowExecutionDto>(dtos, totalCount, request.Page, request.PageSize));
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

    private sealed class WorkflowExecutionFilterSpec : Specification<AiWorkflowExecution>
    {
        public WorkflowExecutionFilterSpec(
            int page, int pageSize, string? status, Guid? userId)
        {
            ApplyCriteria(exec =>
                (status == null || exec.Status == status) &&
                (!userId.HasValue || exec.UserId == userId.Value));

            ApplyOrderByDescending(exec => exec.StartedAt);

            if (pageSize < int.MaxValue)
                ApplyPaging((page - 1) * pageSize, pageSize);
        }
    }
}
