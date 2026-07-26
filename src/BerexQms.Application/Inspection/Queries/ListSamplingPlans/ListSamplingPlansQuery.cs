using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Inspection.DTOs;

namespace BerexQms.Application.Inspection.Queries.ListSamplingPlans;

public sealed record ListSamplingPlansQuery(
    Guid? PartId = null,
    string? InspectionType = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<SamplingPlanDto>>;
