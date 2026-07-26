using Asp.Versioning;
using BerexQms.Application.Inspection.Commands.CreateSamplingPlan;
using BerexQms.Application.Inspection.Commands.ToggleSamplingPlan;
using BerexQms.Application.Inspection.Commands.UpdateSamplingPlan;
using BerexQms.Application.Inspection.Queries.GetSamplingPlanById;
using BerexQms.Application.Inspection.Queries.ListSamplingPlans;
using BerexQms.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BerexQms.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/sampling-plans")]
[Authorize]
public sealed class SamplingPlansController : ControllerBase
{
    private readonly ISender _sender;

    public SamplingPlansController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? partId,
        [FromQuery] string? inspectionType,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListSamplingPlansQuery(partId, inspectionType, isActive, page, pageSize),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSamplingPlanByIdQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSamplingPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateSamplingPlanCommand(
                request.PartId, request.SupplierId, request.InspectionType,
                request.Level, request.AqlValue, request.SampleSize,
                request.AcceptNumber, request.RejectNumber),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateSamplingPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateSamplingPlanCommand(
                id, request.Level, request.AqlValue, request.SampleSize,
                request.AcceptNumber, request.RejectNumber),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ToggleSamplingPlanCommand(id, true), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ToggleSamplingPlanCommand(id, false), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }
}

public sealed record CreateSamplingPlanRequest(
    Guid PartId,
    Guid? SupplierId,
    string InspectionType,
    string Level,
    decimal AqlValue,
    int SampleSize,
    int AcceptNumber,
    int RejectNumber);

public sealed record UpdateSamplingPlanRequest(
    string Level,
    decimal AqlValue,
    int SampleSize,
    int AcceptNumber,
    int RejectNumber);
