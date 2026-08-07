using Asp.Versioning;
using BerexQms.Application.AiEngine.Commands.RecordUserAction;
using BerexQms.Application.AiEngine.Commands.RegisterModel;
using BerexQms.Application.AiEngine.Commands.RequestPrediction;
using BerexQms.Application.AiEngine.Commands.ToggleCapability;
using BerexQms.Application.AiEngine.Commands.TransitionModelStatus;
using BerexQms.Application.AiEngine.Commands.UpdateCapabilityThresholds;
using BerexQms.Application.AiEngine.Queries.GetCapabilityStats;
using BerexQms.Application.AiEngine.Queries.GetInteractionById;
using BerexQms.Application.AiEngine.Queries.GetModelById;
using BerexQms.Application.AiEngine.Queries.ListCapabilityConfigs;
using BerexQms.Application.AiEngine.Queries.ListInteractions;
using BerexQms.Application.AiEngine.Queries.ListModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BerexQms.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ai")]
[Authorize]
public sealed class AiController : ControllerBase
{
    private readonly ISender _sender;

    public AiController(ISender sender)
    {
        _sender = sender;
    }

    // ---- Capabilities ----

    [HttpGet("capabilities")]
    public async Task<IActionResult> ListCapabilities(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListCapabilityConfigsQuery(), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("capabilities/toggle")]
    public async Task<IActionResult> ToggleCapability(
        [FromBody] ToggleCapabilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ToggleCapabilityCommand(request.Capability, request.Enable), cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("capabilities/thresholds")]
    public async Task<IActionResult> UpdateThresholds(
        [FromBody] UpdateThresholdsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateCapabilityThresholdsCommand(
                request.Capability,
                request.LowThreshold,
                request.ModerateThreshold,
                request.HighThreshold),
            cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("capabilities/{capability}/stats")]
    public async Task<IActionResult> GetCapabilityStats(
        string capability,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetCapabilityStatsQuery(capability, days), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    // ---- Predictions / Interactions ----

    [HttpPost("predict")]
    public async Task<IActionResult> RequestPrediction(
        [FromBody] RequestPredictionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RequestPredictionCommand(
                request.Capability,
                request.InputContext,
                request.RelatedRecordId,
                request.RelatedRecordType),
            cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("interactions")]
    public async Task<IActionResult> ListInteractions(
        [FromQuery] string? capability,
        [FromQuery] string? status,
        [FromQuery] string? userAction,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListInteractionsQuery(page, pageSize, capability, status, userAction),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("interactions/{id:guid}")]
    public async Task<IActionResult> GetInteraction(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetInteractionByIdQuery(id), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error.Message });
    }

    [HttpPost("interactions/{id:guid}/action")]
    public async Task<IActionResult> RecordUserAction(
        Guid id,
        [FromBody] RecordUserActionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RecordUserActionCommand(id, request.Action, request.Justification),
            cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error.Message });
    }

    // ---- Models ----

    [HttpGet("models")]
    public async Task<IActionResult> ListModels(
        [FromQuery] string? capability,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListModelsQuery(page, pageSize, capability, status),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("models/{id:guid}")]
    public async Task<IActionResult> GetModel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetModelByIdQuery(id), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : NotFound(new { error = result.Error.Message });
    }

    [HttpPost("models")]
    public async Task<IActionResult> RegisterModel(
        [FromBody] RegisterModelRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RegisterModelCommand(
                request.Name,
                request.Version,
                request.Capability,
                request.Description,
                request.HyperParameters),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetModel), new { id = result.Value }, new { id = result.Value })
            : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("models/{id:guid}/transition")]
    public async Task<IActionResult> TransitionModelStatus(
        Guid id,
        [FromBody] TransitionModelStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new TransitionModelStatusCommand(id, request.TargetStatus),
            cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error.Message });
    }
}

// ---- Request Records ----

public sealed record ToggleCapabilityRequest(string Capability, bool Enable);

public sealed record UpdateThresholdsRequest(
    string Capability, decimal LowThreshold, decimal ModerateThreshold, decimal HighThreshold);

public sealed record RequestPredictionRequest(
    string Capability, string? InputContext, Guid? RelatedRecordId, string? RelatedRecordType);

public sealed record RecordUserActionRequest(string Action, string? Justification);

public sealed record RegisterModelRequest(
    string Name, string Version, string Capability, string? Description, string? HyperParameters);

public sealed record TransitionModelStatusRequest(string TargetStatus);
