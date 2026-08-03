using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.DocumentControl.DTOs;

namespace BerexQms.Application.DocumentControl.Commands.StartApproval;

public sealed record StartApprovalCommand(
    Guid DocumentId,
    Guid VersionId,
    IReadOnlyList<string> ApproverIds) : ICommand<ApprovalWorkflowDto>;
