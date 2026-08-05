using BerexQms.Domain.Spc.Entities;
using BerexQms.Domain.Spc.Enums;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Application.Spc.Specifications;

/// <summary>
/// Filters control charts by free-text search (code/name), chart type, and status,
/// with descending creation-date ordering, paging, and eager loading of data points
/// (so callers can read <c>DataPoints.Count</c> without a separate query).
/// Reused for both the paged listing and its matching total-count query — paging and
/// includes are ignored automatically when the specification is used for a count.
/// </summary>
public sealed class ControlChartFilterSpec : Specification<ControlChart>
{
    public ControlChartFilterSpec(
        string? search, string? chartType, string? status, int page, int pageSize)
    {
        ApplyFilters(search, chartType, status);
        ApplyOrderByDescending(c => c.CreatedAt);
        ApplyPaging(Math.Max(page - 1, 0) * pageSize, pageSize);
        AddInclude(c => c.DataPoints);
    }

    private void ApplyFilters(string? search, string? chartType, string? status)
    {
        var hasSearch = !string.IsNullOrWhiteSpace(search);
        var hasChartType = !string.IsNullOrWhiteSpace(chartType) &&
                            Enum.TryParse<ChartType>(chartType, true, out _);
        var hasStatus = !string.IsNullOrWhiteSpace(status) &&
                         Enum.TryParse<ChartStatus>(status, true, out _);

        if (!hasSearch && !hasChartType && !hasStatus)
            return;

        var term = search?.ToUpperInvariant() ?? string.Empty;
        var parsedChartType = hasChartType ? Enum.Parse<ChartType>(chartType!, true).ToString() : null;
        var parsedStatus = hasStatus ? Enum.Parse<ChartStatus>(status!, true).ToString() : null;

        ApplyCriteria(c =>
            (!hasSearch || c.Code.ToUpper().Contains(term) || c.Name.ToUpper().Contains(term)) &&
            (!hasChartType || c.ChartType == parsedChartType) &&
            (!hasStatus || c.Status == parsedStatus));
    }
}
