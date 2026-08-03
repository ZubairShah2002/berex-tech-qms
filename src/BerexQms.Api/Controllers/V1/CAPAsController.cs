using Asp.Versioning;
using BerexQms.Application.Capa.Commands.AddAction;
using BerexQms.Application.Capa.Commands.AssignCapa;
using BerexQms.Application.Capa.Commands.CompleteAction;
using BerexQms.Application.Capa.Commands.InitiateCapa;
using BerexQms.Application.Capa.Commands.RecordVerification;
using BerexQms.Application.Capa.Commands.ScheduleVerification;
using BerexQms.Application.Capa.Commands.StartRCA;
using BerexQms.Application.Capa.Commands.SubmitRCA;
using BerexQms.Application.Capa.Queries.GetCapaById;
using BerexQms.Application.Capa.Queries.ListCapas;
using BerexQms.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BerexQms.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/capas")]
[Authorize]
public sealed class CAPAsController : ControllerBase
{
    private readonly ISender _sender;

    public CAPAsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? sourceType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListCapasQuery(search, status, priority, sourceType, page, pageSize),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCapaByIdQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCapaRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new InitiateCapaCommand(
                request.CapaNumber, request.Title, request.Description,
                request.Priority, request.SourceType,
                request.SourceNonConformanceId, request.SourceAuditFindingId,
                request.SourceDescription, request.TargetClosureDate),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : result.Error.Type == ErrorType.Conflict
                ? Conflict(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/rca")]
    public async Task<IActionResult> StartRCA(
        Guid id, [FromBody] StartRCARequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new StartRCACommand(id, request.Methodology, request.AnalystId),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}/rca")]
    public async Task<IActionResult> SubmitRCA(
        Guid id, [FromBody] SubmitRCARequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SubmitRCACommand(id, request.RootCause, request.AnalysisDetails, request.ContributingFactors),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/actions")]
    public async Task<IActionResult> AddAction(
        Guid id, [FromBody] AddActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AddActionCommand(
                id, request.ActionType, request.Description,
                request.OwnerId, request.DueDate, request.EvidenceRequirement),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}/actions/{actionId:guid}/complete")]
    public async Task<IActionResult> CompleteAction(
        Guid id, Guid actionId, [FromBody] CompleteActionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CompleteActionCommand(id, actionId, request.CompletionNotes, request.EvidenceProvided),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/verifications")]
    public async Task<IActionResult> ScheduleVerification(
        Guid id, [FromBody] ScheduleVerificationRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ScheduleVerificationCommand(id, request.ScheduledDate, request.VerificationCriteria),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}/verify")]
    public async Task<IActionResult> RecordVerification(
        Guid id, [FromBody] RecordVerificationRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RecordVerificationCommand(
                id, request.VerificationId, request.IsEffective,
                request.Result, request.Evidence),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> Assign(
        Guid id, [FromBody] AssignCapaRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AssignCapaCommand(id, request.AssigneeId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }
}

public sealed record CreateCapaRequest(
    string CapaNumber,
    string Title,
    string Description,
    string Priority,
    string SourceType,
    Guid? SourceNonConformanceId,
    Guid? SourceAuditFindingId,
    string? SourceDescription,
    DateTime? TargetClosureDate);

public sealed record StartRCARequest(string Methodology, string AnalystId);

public sealed record SubmitRCARequest(
    string RootCause,
    string? AnalysisDetails,
    string? ContributingFactors);

public sealed record AddActionRequest(
    string ActionType,
    string Description,
    string OwnerId,
    DateTime DueDate,
    string? EvidenceRequirement);

public sealed record CompleteActionRequest(
    string? CompletionNotes,
    string? EvidenceProvided);

public sealed record ScheduleVerificationRequest(
    DateTime ScheduledDate,
    string VerificationCriteria);

public sealed record RecordVerificationRequest(
    Guid VerificationId,
    bool IsEffective,
    string Result,
    string? Evidence);

public sealed record AssignCapaRequest(string AssigneeId);
