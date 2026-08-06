using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.ListInteractions;

public sealed record ListInteractionsQuery(
    int Page,
    int PageSize,
    string? Capability,
    string? Status,
    string? UserAction) : IQuery<PagedResult<AiInteractionDto>>;
