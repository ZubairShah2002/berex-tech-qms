using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.DocumentControl.Commands.SubmitForReview;

public sealed record SubmitForReviewCommand(
    Guid DocumentId,
    Guid VersionId) : ICommand;
