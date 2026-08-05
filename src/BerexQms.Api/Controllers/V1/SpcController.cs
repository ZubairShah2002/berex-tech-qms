using Asp.Versioning;
using BerexQms.Application.Spc.Commands.AddDataPoint;
using BerexQms.Application.Spc.Commands.CreateChart;
using BerexQms.Application.Spc.Commands.DeactivateChart;
using BerexQms.Application.Spc.Commands.RecalculateLimits;
using BerexQms.Application.Spc.Commands.UpdateChart;
using BerexQms.Application.Spc.Queries.GetChartById;
using BerexQms.Application.Spc.Queries.GetChartsByPart;
using BerexQms.Application.Spc.Queries.ListCharts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BerexQms.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/spc/charts")]
[Authorize]
public sealed class SpcChartsController : ControllerBase
{
    private readonly ISender _sender;

    public SpcChartsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? chartType,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListChartsQuery(search, chartType, status, page, pageSize), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetChartByIdQuery(id), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error.Message });
    }

    [HttpGet("by-part/{partId:guid}")]
    public async Task<IActionResult> GetByPart(Guid partId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetChartsByPartQuery(partId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateChartRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateChartCommand(
            request.Code,
            request.Name,
            request.ChartType,
            request.PartId,
            request.CharacteristicName,
            request.SubgroupSize,
            request.UpperSpecLimit,
            request.LowerSpecLimit), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateChartRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new UpdateChartCommand(
            id,
            request.Name,
            request.SubgroupSize,
            request.UpperSpecLimit,
            request.LowerSpecLimit), cancellationToken);

        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/data-points")]
    public async Task<IActionResult> AddDataPoint(
        Guid id,
        [FromBody] AddDataPointRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AddDataPointCommand(
            id,
            request.Value,
            request.SubgroupValues,
            request.SampleSize,
            request.Timestamp,
            request.InspectionId), cancellationToken);

        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/recalculate")]
    public async Task<IActionResult> RecalculateLimits(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RecalculateLimitsCommand(id), cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateChartCommand(id), cancellationToken);

        return result.IsSuccess ? NoContent() : NotFound(new { error = result.Error.Message });
    }
}

public sealed record CreateChartRequest(
    string Code, string Name, string ChartType, Guid PartId,
    string CharacteristicName, int SubgroupSize,
    decimal? UpperSpecLimit, decimal? LowerSpecLimit);

public sealed record UpdateChartRequest(
    string Name, int SubgroupSize,
    decimal? UpperSpecLimit, decimal? LowerSpecLimit);

public sealed record AddDataPointRequest(
    decimal Value, string? SubgroupValues, int SampleSize,
    DateTime Timestamp, Guid? InspectionId);
