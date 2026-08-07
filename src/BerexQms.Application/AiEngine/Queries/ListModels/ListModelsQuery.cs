using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.ListModels;

public sealed record ListModelsQuery(
    int Page,
    int PageSize,
    string? Capability,
    string? Status) : IQuery<PagedResult<AiModelDto>>;
