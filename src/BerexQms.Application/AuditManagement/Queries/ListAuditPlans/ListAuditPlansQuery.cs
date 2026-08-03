using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AuditManagement.DTOs;

namespace BerexQms.Application.AuditManagement.Queries.ListAuditPlans;

public sealed record ListAuditPlansQuery(
    string? SearchTerm = null,
    int? Year = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<AuditPlanDto>>;
