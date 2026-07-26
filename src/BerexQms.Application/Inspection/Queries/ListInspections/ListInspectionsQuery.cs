using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Abstractions;
using BerexQms.Application.Inspection.DTOs;

namespace BerexQms.Application.Inspection.Queries.ListInspections;

public sealed record ListInspectionsQuery(
    string? SearchTerm = null,
    string? Type = null,
    string? Status = null,
    Guid? PartId = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<InspectionDto>>;
