using Asp.Versioning;
using BerexQms.Application.DocumentControl.Commands.AcknowledgeDistribution;
using BerexQms.Application.DocumentControl.Commands.AddDistribution;
using BerexQms.Application.DocumentControl.Commands.CreateDocument;
using BerexQms.Application.DocumentControl.Commands.CreateVersion;
using BerexQms.Application.DocumentControl.Commands.MakeObsolete;
using BerexQms.Application.DocumentControl.Commands.RecordApproval;
using BerexQms.Application.DocumentControl.Commands.ReleaseVersion;
using BerexQms.Application.DocumentControl.Commands.StartApproval;
using BerexQms.Application.DocumentControl.Commands.SubmitForReview;
using BerexQms.Application.DocumentControl.Queries.GetDocumentById;
using BerexQms.Application.DocumentControl.Queries.ListDocuments;
using BerexQms.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BerexQms.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/documents")]
[Authorize]
public sealed class DocumentsController : ControllerBase
{
    private readonly ISender _sender;

    public DocumentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? documentType,
        [FromQuery] string? status,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListDocumentsQuery(search, documentType, status, isActive, page, pageSize),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetDocumentByIdQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDocumentRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateDocumentCommand(
                request.DocumentNumber, request.Title, request.DocumentType,
                request.Description, request.Department),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : result.Error.Type == ErrorType.Conflict
                ? Conflict(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/versions")]
    public async Task<IActionResult> CreateVersion(
        Guid id, [FromBody] CreateVersionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateVersionCommand(id, request.VersionNumber, request.Content, request.ChangeDescription),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/versions/{versionId:guid}/submit-for-review")]
    public async Task<IActionResult> SubmitForReview(
        Guid id, Guid versionId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new SubmitForReviewCommand(id, versionId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/versions/{versionId:guid}/start-approval")]
    public async Task<IActionResult> StartApproval(
        Guid id, Guid versionId, [FromBody] StartApprovalRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new StartApprovalCommand(id, versionId, request.ApproverIds),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/versions/{versionId:guid}/record-approval")]
    public async Task<IActionResult> RecordApproval(
        Guid id, Guid versionId, [FromBody] RecordApprovalRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RecordApprovalCommand(id, versionId, request.Decision, request.Comments, request.Signature),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/versions/{versionId:guid}/release")]
    public async Task<IActionResult> ReleaseVersion(
        Guid id, Guid versionId, [FromBody] ReleaseVersionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ReleaseVersionCommand(id, versionId, request.EffectiveDate),
            cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/versions/{versionId:guid}/distributions")]
    public async Task<IActionResult> AddDistribution(
        Guid id, Guid versionId, [FromBody] AddDistributionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AddDistributionCommand(id, versionId, request.RecipientId, request.ComplianceDeadline),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/distributions/{distributionId:guid}/acknowledge")]
    public async Task<IActionResult> AcknowledgeDistribution(
        Guid id, Guid distributionId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AcknowledgeDistributionCommand(id, distributionId), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/make-obsolete")]
    public async Task<IActionResult> MakeObsolete(
        Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new MakeObsoleteCommand(id), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }
}

public sealed record CreateDocumentRequest(
    string DocumentNumber,
    string Title,
    string DocumentType,
    string? Description,
    string? Department);

public sealed record CreateVersionRequest(
    string VersionNumber,
    string Content,
    string? ChangeDescription);

public sealed record StartApprovalRequest(List<string> ApproverIds);

public sealed record RecordApprovalRequest(
    string Decision,
    string? Comments,
    string? Signature);

public sealed record ReleaseVersionRequest(DateTime EffectiveDate);

public sealed record AddDistributionRequest(
    string RecipientId,
    DateTime ComplianceDeadline);
