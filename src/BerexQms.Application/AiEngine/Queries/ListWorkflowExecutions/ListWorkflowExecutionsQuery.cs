using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.ListWorkflowExecutions;

public sealed record ListWorkflowExecutionsQuery(
    int Page,
    int PageSize,
    string? Status,
    Guid? UserId) : IQuery<PagedResult<AiWorkflowExecutionDto>>;
