using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Commands.ExecuteAiAction;

/// <summary>
/// Requests execution of an AI action. If the action requires confirmation
/// (dangerous action), returns a <see cref="AiConfirmationRequestDto"/> instead
/// of executing immediately.
/// </summary>
public sealed record ExecuteAiActionCommand(
    string ActionType,
    string? Prompt,
    string? TargetModule,
    string? TargetRecordId,
    string? TargetRecordType,
    string? Parameters) : ICommand<ExecuteAiActionResult>;

/// <summary>
/// Discriminated result: either an immediate execution result or a confirmation request.
/// </summary>
public sealed record ExecuteAiActionResult(
    bool RequiresConfirmation,
    AiConfirmationRequestDto? ConfirmationRequest,
    AiActionLogDto? ActionResult);
