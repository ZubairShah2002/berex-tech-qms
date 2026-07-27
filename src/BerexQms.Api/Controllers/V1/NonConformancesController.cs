using Asp.Versioning;
using BerexQms.Application.NonConformance.Commands.AddContainmentAction;
using BerexQms.Application.NonConformance.Commands.AssignInvestigator;
using BerexQms.Application.NonConformance.Commands.CloseAsDuplicate;
using BerexQms.Application.NonConformance.Commands.CreateNonConformance;
using BerexQms.Application.NonConformance.Commands.LinkCapa;
using BerexQms.Application.NonConformance.Commands.RecordDisposition;
using BerexQms.Application.NonConformance.Commands.ReopenNonConformance;
using BerexQms.Application.NonConformance.Commands.RequestMoreInfo;
using BerexQms.Application.NonConformance.Commands.SubmitInvestigation;
using BerexQms.Application.NonConformance.Commands.VerifyContainment;
using BerexQms.Application.NonConformance.Queries.FindSimilarNonConformances;
using BerexQms.Application.NonConformance.Queries.GetNonConformanceById;
using BerexQms.Application.NonConformance.Queries.ListNonConformances;
using BerexQms.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BerexQms.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/non-conformances")]
[Authorize]
public sealed class NonConformancesController : ControllerBase
{
    private readonly ISender _sender;

    public NonConformancesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? severity,
        [FromQuery] string? source,
        [FromQuery] Guid? partId,
        [FromQuery] Guid? supplierId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListNonConformancesQuery(search, status, severity, source, partId, supplierId, page, pageSize),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetNonConformanceByIdQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id:guid}/similar")]
    public async Task<IActionResult> FindSimilar(
        Guid id,
        [FromQuery] int lookbackDays = 90,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new FindSimilarNonConformancesQuery(id, lookbackDays),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateNonConformanceRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateNonConformanceCommand(
                request.NcrNumber, request.Severity, request.Source,
                request.DetectionPoint, request.Description, request.PartId,
                request.PartRevisionId, request.LotNumber, request.SerialNumber,
                request.SupplierId, request.SupplierLotNumber, request.WorkOrderNumber,
                request.CustomerId, request.SourceInspectionId,
                request.QuantityAffected, request.QuantityDefective),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : result.Error.Type == ErrorType.Conflict
                ? Conflict(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/assign-investigator")]
    public async Task<IActionResult> AssignInvestigator(
        Guid id, [FromBody] AssignInvestigatorRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AssignInvestigatorCommand(id, request.InvestigatorId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/containment-actions")]
    public async Task<IActionResult> AddContainmentAction(
        Guid id, [FromBody] AddContainmentActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AddContainmentActionCommand(id, request.Description), cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/containment-actions/{actionId:guid}/verify")]
    public async Task<IActionResult> VerifyContainment(
        Guid id, Guid actionId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new VerifyContainmentCommand(id, actionId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}/investigation")]
    public async Task<IActionResult> SubmitInvestigation(
        Guid id, [FromBody] SubmitInvestigationRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SubmitInvestigationCommand(id, request.Methodology, request.RootCause, request.Findings),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}/disposition")]
    public async Task<IActionResult> RecordDisposition(
        Guid id, [FromBody] RecordDispositionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RecordDispositionCommand(id, request.Type, request.Justification), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/request-more-info")]
    public async Task<IActionResult> RequestMoreInfo(
        Guid id, [FromBody] RequestMoreInfoRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RequestMoreInfoCommand(id, request.Reason), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/close-as-duplicate")]
    public async Task<IActionResult> CloseAsDuplicate(
        Guid id, [FromBody] CloseAsDuplicateRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CloseAsDuplicateCommand(id, request.Notes), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/reopen")]
    public async Task<IActionResult> Reopen(
        Guid id, [FromBody] ReopenRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ReopenNonConformanceCommand(id, request.Reason), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/link-capa")]
    public async Task<IActionResult> LinkCapa(
        Guid id, [FromBody] LinkCapaRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new LinkCapaCommand(id, request.CapaId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }
}

public sealed record CreateNonConformanceRequest(
    string NcrNumber,
    string Severity,
    string Source,
    string DetectionPoint,
    string Description,
    Guid PartId,
    Guid? PartRevisionId,
    string? LotNumber,
    string? SerialNumber,
    Guid? SupplierId,
    string? SupplierLotNumber,
    string? WorkOrderNumber,
    Guid? CustomerId,
    Guid? SourceInspectionId,
    int QuantityAffected,
    int QuantityDefective);

public sealed record AssignInvestigatorRequest(string InvestigatorId);

public sealed record AddContainmentActionRequest(string Description);

public sealed record SubmitInvestigationRequest(
    string RootCause,
    string Findings,
    string? Methodology);

public sealed record RecordDispositionRequest(string Type, string Justification);

public sealed record RequestMoreInfoRequest(string Reason);

public sealed record CloseAsDuplicateRequest(string Notes);

public sealed record ReopenRequest(string Reason);

public sealed record LinkCapaRequest(Guid CapaId);
