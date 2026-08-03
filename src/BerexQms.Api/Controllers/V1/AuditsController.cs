using Asp.Versioning;
using BerexQms.Application.AuditManagement.Commands.AddAudit;
using BerexQms.Application.AuditManagement.Commands.AddChecklist;
using BerexQms.Application.AuditManagement.Commands.CancelAudit;
using BerexQms.Application.AuditManagement.Commands.CompleteAudit;
using BerexQms.Application.AuditManagement.Commands.CreateAuditPlan;
using BerexQms.Application.AuditManagement.Commands.RecordFinding;
using BerexQms.Application.AuditManagement.Commands.StartAudit;
using BerexQms.Application.AuditManagement.Queries.GetAuditPlanById;
using BerexQms.Application.AuditManagement.Queries.ListAuditPlans;
using BerexQms.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BerexQms.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/audits")]
[Authorize]
public sealed class AuditsController : ControllerBase
{
    private readonly ISender _sender;

    public AuditsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] int? year,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListAuditPlansQuery(search, year, isActive, page, pageSize),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAuditPlanByIdQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAuditPlanRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateAuditPlanCommand(request.PlanName, request.Year, request.Description, request.Scope),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : result.Error.Type == ErrorType.Conflict
                ? Conflict(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/audits")]
    public async Task<IActionResult> AddAudit(
        Guid id, [FromBody] AddAuditRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AddAuditCommand(
                id, request.AuditNumber, request.AuditType,
                request.LeadAuditorId, request.AuditeeArea, request.ScheduledDate),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}/audits/{auditId:guid}/start")]
    public async Task<IActionResult> StartAudit(
        Guid id, Guid auditId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new StartAuditCommand(id, auditId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}/audits/{auditId:guid}/complete")]
    public async Task<IActionResult> CompleteAudit(
        Guid id, Guid auditId, [FromBody] CompleteAuditRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CompleteAuditCommand(id, auditId, request.Summary, request.Recommendations, request.AuditorNotes),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}/audits/{auditId:guid}/cancel")]
    public async Task<IActionResult> CancelAudit(
        Guid id, Guid auditId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelAuditCommand(id, auditId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/audits/{auditId:guid}/findings")]
    public async Task<IActionResult> RecordFinding(
        Guid id, Guid auditId, [FromBody] RecordFindingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RecordFindingCommand(
                id, auditId, request.Classification, request.ClauseReference,
                request.Description, request.Evidence, request.CorrectiveAction, request.LinkedCapaId),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/audits/{auditId:guid}/checklists")]
    public async Task<IActionResult> AddChecklist(
        Guid id, Guid auditId, [FromBody] AddChecklistRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AddChecklistCommand(
                id, auditId, request.Standard, request.ClauseReference,
                request.Requirement, request.IsCompliant, request.Evidence, request.Notes),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }
}

public sealed record CreateAuditPlanRequest(
    string PlanName,
    int Year,
    string? Description,
    string? Scope);

public sealed record AddAuditRequest(
    string AuditNumber,
    string AuditType,
    string LeadAuditorId,
    string? AuditeeArea,
    DateTime ScheduledDate);

public sealed record CompleteAuditRequest(
    string Summary,
    string Recommendations,
    string? AuditorNotes);

public sealed record RecordFindingRequest(
    string Classification,
    string ClauseReference,
    string Description,
    string? Evidence,
    string? CorrectiveAction,
    string? LinkedCapaId);

public sealed record AddChecklistRequest(
    string Standard,
    string ClauseReference,
    string Requirement,
    bool IsCompliant,
    string? Evidence,
    string? Notes);
