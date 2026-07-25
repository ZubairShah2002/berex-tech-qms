using Asp.Versioning;
using BerexQms.Application.ProductCatalog.Commands.AddBomReference;
using BerexQms.Application.ProductCatalog.Commands.AddSpecificationParameter;
using BerexQms.Application.ProductCatalog.Commands.CreatePart;
using BerexQms.Application.ProductCatalog.Commands.CreatePartRevision;
using BerexQms.Application.ProductCatalog.Commands.ObsoletePart;
using BerexQms.Application.ProductCatalog.Commands.ReleaseRevision;
using BerexQms.Application.ProductCatalog.Commands.RemoveBomReference;
using BerexQms.Application.ProductCatalog.Commands.UpdatePart;
using BerexQms.Application.ProductCatalog.Queries.GetPartById;
using BerexQms.Application.ProductCatalog.Queries.ListParts;
using BerexQms.SharedKernel.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BerexQms.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/parts")]
[Authorize]
public sealed class PartsController : ControllerBase
{
    private readonly ISender _sender;

    public PartsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? productFamily,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListPartsQuery(search, status, productFamily, category, page, pageSize),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPartByIdQuery(id), cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePartRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreatePartCommand(
                request.PartNumber, request.Name, request.Description,
                request.ProductFamily, request.Category, request.SerializationMode,
                request.UnitOfMeasure),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : result.Error.Type == ErrorType.Conflict
                ? Conflict(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePartRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdatePartCommand(
                id, request.Name, request.Description,
                request.ProductFamily, request.Category, request.SerializationMode,
                request.UnitOfMeasure),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/obsolete")]
    public async Task<IActionResult> Obsolete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ObsoletePartCommand(id), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/revisions")]
    public async Task<IActionResult> CreateRevision(
        Guid id, [FromBody] CreateRevisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreatePartRevisionCommand(id, request.RevisionCode, request.Description, request.ChangeReason),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/revisions/{revisionId:guid}/release")]
    public async Task<IActionResult> ReleaseRevision(Guid id, Guid revisionId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReleaseRevisionCommand(id, revisionId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/revisions/{revisionId:guid}/parameters")]
    public async Task<IActionResult> AddSpecificationParameter(
        Guid id, Guid revisionId, [FromBody] AddParameterRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AddSpecificationParameterCommand(
                id, revisionId, request.Name, request.Type, request.Unit,
                request.NominalValue, request.UpperTolerance, request.LowerTolerance,
                request.TextValue, request.IsCritical),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id:guid}/bom")]
    public async Task<IActionResult> AddBomReference(
        Guid id, [FromBody] AddBomReferenceRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AddBomReferenceCommand(id, request.ChildPartId, request.Quantity, request.ReferenceDesignator),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id }, result.Value)
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : result.Error.Type == ErrorType.Conflict
                    ? Conflict(new { error = result.Error.Message })
                    : BadRequest(new { error = result.Error.Message });
    }

    [HttpDelete("{id:guid}/bom/{bomReferenceId:guid}")]
    public async Task<IActionResult> RemoveBomReference(Guid id, Guid bomReferenceId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemoveBomReferenceCommand(id, bomReferenceId), cancellationToken);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == ErrorType.NotFound
                ? NotFound(new { error = result.Error.Message })
                : BadRequest(new { error = result.Error.Message });
    }
}

public sealed record CreatePartRequest(
    string PartNumber,
    string Name,
    string? Description,
    string? ProductFamily,
    string? Category,
    string SerializationMode,
    string? UnitOfMeasure);

public sealed record UpdatePartRequest(
    string Name,
    string? Description,
    string? ProductFamily,
    string? Category,
    string SerializationMode,
    string? UnitOfMeasure);

public sealed record CreateRevisionRequest(
    string RevisionCode,
    string? Description,
    string? ChangeReason);

public sealed record AddParameterRequest(
    string Name,
    string Type,
    string? Unit,
    decimal? NominalValue,
    decimal? UpperTolerance,
    decimal? LowerTolerance,
    string? TextValue,
    bool IsCritical);

public sealed record AddBomReferenceRequest(
    Guid ChildPartId,
    decimal Quantity,
    string? ReferenceDesignator);
