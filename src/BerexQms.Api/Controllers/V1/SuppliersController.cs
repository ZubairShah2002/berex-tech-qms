using Asp.Versioning;
using BerexQms.Application.SupplierQuality.Commands.AddApproval;
using BerexQms.Application.SupplierQuality.Commands.AddApprovedPart;
using BerexQms.Application.SupplierQuality.Commands.CreateScorecard;
using BerexQms.Application.SupplierQuality.Commands.CreateSupplier;
using BerexQms.Application.SupplierQuality.Commands.IssueScar;
using BerexQms.Application.SupplierQuality.Commands.RespondToScar;
using BerexQms.Application.SupplierQuality.Commands.ReviewScarResponse;
using BerexQms.Application.SupplierQuality.Commands.UpdateSupplier;
using BerexQms.Application.SupplierQuality.Commands.VerifyScar;
using BerexQms.Application.SupplierQuality.Queries.GetSupplierById;
using BerexQms.Application.SupplierQuality.Queries.ListSuppliers;
using BerexQms.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BerexQms.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/suppliers")]
[Authorize]
public sealed class SuppliersController : ControllerBase
{
    private readonly ISender _sender;

    public SuppliersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? riskLevel,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListSuppliersQuery(search, status, riskLevel, page, pageSize),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSupplierByIdQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSupplierRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateSupplierCommand(
                request.Code, request.Name, request.Tier,
                request.ContactName, request.ContactRole, request.ContactEmail, request.ContactPhone),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : result.Error.Type == ErrorType.Conflict
                ? Conflict(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateSupplierCommand(
                id, request.Name, request.Tier,
                request.ContactName, request.ContactRole, request.ContactEmail, request.ContactPhone),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/approvals")]
    public async Task<IActionResult> AddApproval(
        Guid id, [FromBody] AddApprovalRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AddApprovalCommand(
                id, request.ScopeDescription, request.ApprovedDate, request.ExpiryDate, request.Conditions),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/scorecards")]
    public async Task<IActionResult> CreateScorecard(
        Guid id, [FromBody] CreateScorecardRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateScorecardCommand(
                id, request.PeriodStart, request.PeriodEnd,
                request.QualityScore, request.DeliveryScore, request.ResponsivenessScore, request.CostScore),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/scars")]
    public async Task<IActionResult> IssueScar(
        Guid id, [FromBody] IssueScarRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new IssueScarCommand(
                id, request.ScarNumber, request.NonConformanceId,
                request.DefectDescription, request.Severity, request.ResponseDays),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}/scars/{scarId:guid}/respond")]
    public async Task<IActionResult> RespondToScar(
        Guid id, Guid scarId, [FromBody] RespondToScarRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RespondToScarCommand(
                id, scarId, request.RootCause, request.CorrectiveActions, request.EvidenceRefs),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}/scars/{scarId:guid}/review")]
    public async Task<IActionResult> ReviewScarResponse(
        Guid id, Guid scarId, [FromBody] ReviewScarRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ReviewScarResponseCommand(id, scarId, request.Decision),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}/scars/{scarId:guid}/verify")]
    public async Task<IActionResult> VerifyScar(
        Guid id, Guid scarId, [FromBody] VerifyScarRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new VerifyScarCommand(id, scarId, request.Action),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/approved-parts")]
    public async Task<IActionResult> AddApprovedPart(
        Guid id, [FromBody] AddApprovedPartRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AddApprovedPartCommand(id, request.PartId, request.RevisionScope, request.ApprovalDate),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }
}

public sealed record CreateSupplierRequest(
    string Code,
    string Name,
    string? Tier,
    string? ContactName,
    string? ContactRole,
    string? ContactEmail,
    string? ContactPhone);

public sealed record UpdateSupplierRequest(
    string Name,
    string? Tier,
    string? ContactName,
    string? ContactRole,
    string? ContactEmail,
    string? ContactPhone);

public sealed record AddApprovalRequest(
    string ScopeDescription,
    DateTime ApprovedDate,
    DateTime? ExpiryDate,
    string? Conditions);

public sealed record CreateScorecardRequest(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal QualityScore,
    decimal DeliveryScore,
    decimal ResponsivenessScore,
    decimal CostScore);

public sealed record IssueScarRequest(
    string ScarNumber,
    Guid? NonConformanceId,
    string DefectDescription,
    string Severity,
    int ResponseDays = 14);

public sealed record RespondToScarRequest(
    string RootCause,
    string CorrectiveActions,
    string? EvidenceRefs);

public sealed record ReviewScarRequest(string Decision);

public sealed record VerifyScarRequest(string Action);

public sealed record AddApprovedPartRequest(
    Guid PartId,
    string? RevisionScope,
    DateTime ApprovalDate);
