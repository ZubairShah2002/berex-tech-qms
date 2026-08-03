using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.DocumentControl.Commands.ReleaseVersion;

public sealed record ReleaseVersionCommand(
    Guid DocumentId,
    Guid VersionId,
    DateTime EffectiveDate) : ICommand;
