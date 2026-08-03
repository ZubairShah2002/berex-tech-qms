using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.DocumentControl.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.DocumentControl.Commands.SubmitForReview;

internal sealed class SubmitForReviewCommandHandler : ICommandHandler<SubmitForReviewCommand>
{
    private readonly IDocumentRepository _repository;

    public SubmitForReviewCommandHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(SubmitForReviewCommand request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetWithVersionsAsync(request.DocumentId, cancellationToken);
        if (document is null)
            return Result.Failure(DocumentErrors.NotFound);

        document.SubmitVersionForReview(request.VersionId);
        return Result.Success();
    }
}
