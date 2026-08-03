using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Capa.DTOs;

namespace BerexQms.Application.Capa.Queries.ListCapas;

public sealed record ListCapasQuery(
    string? SearchTerm = null,
    string? Status = null,
    string? Priority = null,
    string? SourceType = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<CAPADto>>;
