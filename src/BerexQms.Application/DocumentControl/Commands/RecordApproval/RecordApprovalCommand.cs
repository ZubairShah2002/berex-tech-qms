using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.DocumentControl.Commands.RecordApproval;

public sealed record RecordApprovalCommand(
    Guid DocumentId,
    Guid VersionId,
    string Decision,
    string? Comments,
    string? Signature) : ICommand;
