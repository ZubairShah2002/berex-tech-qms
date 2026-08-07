using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.ListAiActionLogs;

public sealed record ListAiActionLogsQuery(
    int Page,
    int PageSize,
    string? ActionType,
    string? PermissionLevel,
    string? ExecutionResult,
    Guid? UserId) : IQuery<PagedResult<AiActionLogDto>>;
