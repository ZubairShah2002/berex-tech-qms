using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Commands.ExecuteWorkflow;

/// <summary>
/// Initiates execution of an AI workflow. All workflows require confirmation,
/// so this always returns a pending execution that needs to be confirmed via
/// <see cref="ConfirmWorkflow.ConfirmWorkflowCommand"/>.
/// </summary>
public sealed record ExecuteWorkflowCommand(
    Guid WorkflowDefinitionId) : ICommand<AiWorkflowExecutionDto>;
