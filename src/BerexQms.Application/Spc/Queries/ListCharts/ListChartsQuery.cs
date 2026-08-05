using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Spc.DTOs;

namespace BerexQms.Application.Spc.Queries.ListCharts;

public sealed record ListChartsQuery(
    string? Search,
    string? ChartType,
    string? Status,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<ControlChartDto>>;
