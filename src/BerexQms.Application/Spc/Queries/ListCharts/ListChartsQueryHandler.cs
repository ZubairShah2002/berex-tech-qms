using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Spc.DTOs;
using BerexQms.Application.Spc.Specifications;
using BerexQms.Domain.Spc.Entities;
using BerexQms.Domain.Spc.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Spc.Queries.ListCharts;

internal sealed class ListChartsQueryHandler : IQueryHandler<ListChartsQuery, PagedResult<ControlChartDto>>
{
    private readonly IControlChartRepository _repository;

    public ListChartsQueryHandler(IControlChartRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<ControlChartDto>>> Handle(
        ListChartsQuery request, CancellationToken cancellationToken)
    {
        var spec = new ControlChartFilterSpec(
            request.Search, request.ChartType, request.Status, request.Page, request.PageSize);

        var charts = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);

        var dtos = charts.Select(MapToDto).ToList();

        return new PagedResult<ControlChartDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    internal static ControlChartDto MapToDto(ControlChart chart)
    {
        ControlLimitsDto? limitsDto = chart.ControlLimits is null
            ? null
            : new ControlLimitsDto(
                chart.ControlLimits.UpperControlLimit,
                chart.ControlLimits.CenterLine,
                chart.ControlLimits.LowerControlLimit,
                chart.ControlLimits.UpperSpecLimit,
                chart.ControlLimits.LowerSpecLimit);

        ProcessCapabilityDto? capabilityDto = chart.ProcessCapability is null
            ? null
            : new ProcessCapabilityDto(
                chart.ProcessCapability.Cp,
                chart.ProcessCapability.Cpk,
                chart.ProcessCapability.Pp,
                chart.ProcessCapability.Ppk,
                chart.ProcessCapability.Mean,
                chart.ProcessCapability.StdDev,
                chart.ProcessCapability.SampleSize,
                chart.ProcessCapability.CalculatedAt);

        return new ControlChartDto(
            chart.Id,
            chart.Code,
            chart.Name,
            chart.ChartType,
            chart.PartId,
            chart.CharacteristicName,
            chart.SubgroupSize,
            chart.Status,
            chart.IsActive,
            limitsDto,
            capabilityDto,
            chart.UpperSpecLimit,
            chart.LowerSpecLimit,
            chart.DataPoints.Count,
            chart.CreatedAt);
    }
}
