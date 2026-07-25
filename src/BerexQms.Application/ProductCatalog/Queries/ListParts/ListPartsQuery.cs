using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.ProductCatalog.DTOs;

namespace BerexQms.Application.ProductCatalog.Queries.ListParts;

public sealed record ListPartsQuery(
    string? SearchTerm = null,
    string? Status = null,
    string? ProductFamily = null,
    string? Category = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<PartDto>>;
