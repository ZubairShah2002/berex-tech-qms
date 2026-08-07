using Asp.Versioning;
using BerexQms.Application.AiEngine.Commands.AssignAiPermissionLevel;
using BerexQms.Application.AiEngine.Commands.ConfirmAiAction;
using BerexQms.Application.AiEngine.Commands.ConfirmWorkflow;
using BerexQms.Application.AiEngine.Commands.CreateContextDocument;
using BerexQms.Application.AiEngine.Commands.ExecuteAiAction;
using BerexQms.Application.AiEngine.Commands.ExecuteWorkflow;
using BerexQms.Application.AiEngine.Commands.IndexContextDocument;
using BerexQms.Application.AiEngine.Commands.RecordUserAction;
using BerexQms.Application.AiEngine.Commands.RegisterModel;
using BerexQms.Application.AiEngine.Commands.RequestPrediction;
using BerexQms.Application.AiEngine.Commands.RevokeAiPermission;
using BerexQms.Application.AiEngine.Commands.ToggleCapability;
using BerexQms.Application.AiEngine.Commands.TransitionModelStatus;
using BerexQms.Application.AiEngine.Commands.UpdateCapabilityThresholds;
using BerexQms.Application.AiEngine.Commands.UpdateContextDocument;
using BerexQms.Application.AiEngine.Queries.GetAiContext;
using BerexQms.Application.AiEngine.Queries.GetCapabilityStats;
using BerexQms.Application.AiEngine.Queries.GetContextStats;
using BerexQms.Application.AiEngine.Queries.GetInteractionById;
using BerexQms.Application.AiEngine.Queries.GetModelById;
using BerexQms.Application.AiEngine.Queries.GetUserAiPermissions;
using BerexQms.Application.AiEngine.Queries.ListAiActionLogs;
using BerexQms.Application.AiEngine.Queries.ListCapabilityConfigs;
using BerexQms.Application.AiEngine.Queries.ListInteractions;
using BerexQms.Application.AiEngine.Queries.ListKnowledgeSources;
using BerexQms.Application.AiEngine.Queries.ListModels;
using BerexQms.Application.AiEngine.Queries.ListWorkflowDefinitions;
using BerexQms.Application.AiEngine.Queries.ListWorkflowExecutions;
using BerexQms.Application.AiEngine.Queries.SearchKnowledgeContext;
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

    // ---- AI Permissions ----

    [HttpGet("permissions/{userId:guid}")]
    public async Task<IActionResult> GetUserPermissions(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new GetUserAiPermissionsQuery(userId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("permissions/assign")]
    public async Task<IActionResult> AssignPermissionLevel(
        [FromBody] AssignPermissionLevelRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AssignAiPermissionLevelCommand(request.UserId, request.PermissionLevel, request.Notes),
            cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("permissions/revoke")]
    public async Task<IActionResult> RevokePermission(
        [FromBody] RevokePermissionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RevokeAiPermissionCommand(request.UserId), cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error.Message });
    }

    // ---- AI Actions ----

    [HttpPost("actions/execute")]
    public async Task<IActionResult> ExecuteAction(
        [FromBody] ExecuteActionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ExecuteAiActionCommand(
                request.ActionType,
                request.Prompt,
                request.TargetModule,
                request.TargetRecordId,
                request.TargetRecordType,
                request.Parameters),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("actions/{actionLogId:guid}/confirm")]
    public async Task<IActionResult> ConfirmAction(
        Guid actionLogId,
        [FromBody] ConfirmActionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ConfirmAiActionCommand(actionLogId, request.Confirm), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("actions/logs")]
    public async Task<IActionResult> ListActionLogs(
        [FromQuery] string? actionType,
        [FromQuery] string? permissionLevel,
        [FromQuery] string? executionResult,
        [FromQuery] Guid? userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListAiActionLogsQuery(page, pageSize, actionType, permissionLevel, executionResult, userId),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    // ---- Workflows ----

    [HttpGet("workflows/definitions")]
    public async Task<IActionResult> ListWorkflowDefinitions(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListWorkflowDefinitionsQuery(activeOnly), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("workflows/execute")]
    public async Task<IActionResult> ExecuteWorkflow(
        [FromBody] ExecuteWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ExecuteWorkflowCommand(request.WorkflowDefinitionId), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("workflows/executions/{executionId:guid}/confirm")]
    public async Task<IActionResult> ConfirmWorkflow(
        Guid executionId,
        [FromBody] ConfirmWorkflowRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new ConfirmWorkflowCommand(executionId, request.Confirm), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("workflows/executions")]
    public async Task<IActionResult> ListWorkflowExecutions(
        [FromQuery] string? status,
        [FromQuery] Guid? userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListWorkflowExecutionsQuery(page, pageSize, status, userId),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    // ---- Knowledge Context ----

    [HttpGet("context")]
    public async Task<IActionResult> GetContext(
        [FromQuery] string sourceModule,
        [FromQuery] string? contextType,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new GetAiContextQuery(sourceModule, contextType), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("context/stats")]
    public async Task<IActionResult> GetContextStats(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetContextStatsQuery(), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("context/search")]
    public async Task<IActionResult> SearchContext(
        [FromQuery] string searchTerm,
        [FromQuery] string? sourceModule,
        [FromQuery] int maxResults = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new SearchKnowledgeContextQuery(searchTerm, sourceModule, maxResults),
            cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("context/documents")]
    public async Task<IActionResult> CreateContextDocument(
        [FromBody] CreateContextDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateContextDocumentCommand(
                request.SourceModule,
                request.SourceEntityId,
                request.ContextType,
                request.Title,
                request.Content,
                request.MetadataJson),
            cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetContext), new { sourceModule = request.SourceModule }, new { id = result.Value })
            : BadRequest(new { error = result.Error.Message });
    }

    [HttpPut("context/documents/{id:guid}")]
    public async Task<IActionResult> UpdateContextDocument(
        Guid id,
        [FromBody] UpdateContextDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateContextDocumentCommand(id, request.Title, request.Content, request.MetadataJson),
            cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("context/documents/{id:guid}/index")]
    public async Task<IActionResult> IndexContextDocument(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new IndexContextDocumentCommand(id), cancellationToken);

        return result.IsSuccess ? NoContent() : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("knowledge-sources")]
    public async Task<IActionResult> ListKnowledgeSources(
        [FromQuery] bool? activeOnly,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ListKnowledgeSourcesQuery(activeOnly), cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { error = result.Error.Message });
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

public sealed record AssignPermissionLevelRequest(Guid UserId, string PermissionLevel, string? Notes);

public sealed record RevokePermissionRequest(Guid UserId);

public sealed record ExecuteActionRequest(
    string ActionType, string? Prompt, string? TargetModule,
    string? TargetRecordId, string? TargetRecordType, string? Parameters);

public sealed record ConfirmActionRequest(bool Confirm);

public sealed record ExecuteWorkflowRequest(Guid WorkflowDefinitionId);

public sealed record ConfirmWorkflowRequest(bool Confirm);

public sealed record CreateContextDocumentRequest(
    string SourceModule, string? SourceEntityId, string ContextType,
    string Title, string Content, string? MetadataJson);

public sealed record UpdateContextDocumentRequest(
    string Title, string Content, string? MetadataJson);
