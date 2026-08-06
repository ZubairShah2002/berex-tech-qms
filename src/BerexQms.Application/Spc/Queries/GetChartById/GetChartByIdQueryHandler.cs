using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Spc.DTOs;
using BerexQms.Domain.Spc.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.Spc.Queries.GetChartById;

internal sealed class GetChartByIdQueryHandler : IQueryHandler<GetChartByIdQuery, ControlChartDetailDto>
{
    private readonly IControlChartRepository _repository;

    public GetChartByIdQueryHandler(IControlChartRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ControlChartDetailDto>> Handle(
        GetChartByIdQuery request, CancellationToken cancellationToken)
    {
        var chart = await _repository.GetWithDataPointsAsync(request.Id, cancellationToken);
        if (chart is null)
            return SpcErrors.ChartNotFound;

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

        var dataPoints = chart.DataPoints
            .OrderBy(p => p.Timestamp)
            .Select(p => new DataPointDto(
                p.Id,
                p.Value,
                p.SubgroupValues,
                p.SampleSize,
                p.Timestamp,
                p.InspectionId,
                p.RuleViolation,
                p.IsOutOfControl))
            .ToList();

        return new ControlChartDetailDto(
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
            dataPoints,
            chart.CreatedAt);
    }
}
