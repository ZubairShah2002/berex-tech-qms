using Asp.Versioning;
using BerexQms.Application.Inspection.Commands.ApproveInspection;
using BerexQms.Application.Inspection.Commands.CancelInspection;
using BerexQms.Application.Inspection.Commands.CompleteInspection;
using BerexQms.Application.Inspection.Commands.CreateInspection;
using BerexQms.Application.Inspection.Commands.RecordMeasurement;
using BerexQms.Application.Inspection.Commands.RejectInspection;
using BerexQms.Application.Inspection.Commands.SetDisposition;
using BerexQms.Application.Inspection.Commands.StartInspection;
using BerexQms.Application.Inspection.Queries.GetInspectionById;
using BerexQms.Application.Inspection.Queries.ListInspections;
using BerexQms.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BerexQms.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/inspections")]
public sealed class InspectionsController : ControllerBase
{
    private readonly ISender _sender;

    public InspectionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? type,
        [FromQuery] string? status,
        [FromQuery] Guid? partId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListInspectionsQuery(search, type, status, partId, page, pageSize),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetInspectionByIdQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost]
    [Authorize(Roles = "Administrator,SuperAdministrator,QualityManager,QualityEngineer,Inspector")]
    public async Task<IActionResult> Create(
        [FromBody] CreateInspectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateInspectionCommand(
                request.InspectionNumber, request.Type, request.PartId,
                request.PartRevisionId, request.LotNumber, request.LotSize,
                request.SampleSize, request.SupplierId, request.SamplingPlanId,
                request.InspectorId),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : result.Error.Type == ErrorType.Conflict
                ? Conflict(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/start")]
    [Authorize(Roles = "Administrator,SuperAdministrator,QualityManager,QualityEngineer,Inspector")]
    public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new StartInspectionCommand(id), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/measurements")]
    [Authorize(Roles = "Administrator,SuperAdministrator,QualityManager,QualityEngineer,Inspector")]
    public async Task<IActionResult> RecordMeasurement(
        Guid id, [FromBody] RecordMeasurementRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RecordMeasurementCommand(
                id, request.ChecklistItemId, request.CharacteristicName,
                request.MeasuredValue, request.TextValue, request.Unit,
                request.Result, request.EquipmentId, request.OperatorId),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Administrator,SuperAdministrator,QualityManager,QualityEngineer,Inspector")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CompleteInspectionCommand(id), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Administrator,SuperAdministrator,QualityManager,QualityEngineer")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ApproveInspectionCommand(id), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Administrator,SuperAdministrator,QualityManager,QualityEngineer")]
    public async Task<IActionResult> Reject(
        Guid id, [FromBody] RejectInspectionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RejectInspectionCommand(id, request.Notes), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/disposition")]
    [Authorize(Roles = "Administrator,SuperAdministrator,QualityManager,QualityEngineer")]
    public async Task<IActionResult> SetDisposition(
        Guid id, [FromBody] SetDispositionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SetDispositionCommand(id, request.Type, request.Justification),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Administrator,SuperAdministrator,QualityManager")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelInspectionCommand(id), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }
}

public sealed record CreateInspectionRequest(
    string InspectionNumber,
    string Type,
    Guid PartId,
    Guid? PartRevisionId,
    string? LotNumber,
    int? LotSize,
    int? SampleSize,
    Guid? SupplierId,
    Guid? SamplingPlanId,
    string InspectorId);

public sealed record RecordMeasurementRequest(
    Guid? ChecklistItemId,
    string CharacteristicName,
    decimal? MeasuredValue,
    string? TextValue,
    string? Unit,
    string Result,
    Guid? EquipmentId,
    string? OperatorId);

public sealed record RejectInspectionRequest(string? Notes);

public sealed record SetDispositionRequest(string Type, string Justification);
