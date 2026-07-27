using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.NonConformance.DTOs;

namespace BerexQms.Application.NonConformance.Queries.ListNonConformances;

public sealed record ListNonConformancesQuery(
    string? SearchTerm = null,
    string? Status = null,
    string? Severity = null,
    string? Source = null,
    Guid? PartId = null,
    Guid? SupplierId = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<NonConformanceDto>>;
