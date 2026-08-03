using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.DocumentControl.DTOs;

namespace BerexQms.Application.DocumentControl.Commands.CreateVersion;

public sealed record CreateVersionCommand(
    Guid DocumentId,
    string VersionNumber,
    string Content,
    string? ChangeDescription) : ICommand<DocumentVersionDto>;
