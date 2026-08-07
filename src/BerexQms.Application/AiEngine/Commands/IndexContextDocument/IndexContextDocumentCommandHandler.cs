using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.IndexContextDocument;

internal sealed class IndexContextDocumentCommandHandler
    : ICommandHandler<IndexContextDocumentCommand>
{
    private readonly IAiContextDocumentRepository _repository;
    private readonly IEmbeddingService _embeddingService;

    public IndexContextDocumentCommandHandler(
        IAiContextDocumentRepository repository,
        IEmbeddingService embeddingService)
    {
        _repository = repository;
        _embeddingService = embeddingService;
    }

    public async Task<Result> Handle(
        IndexContextDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null)
            return Result.Failure(AiEngineErrors.ContextDocumentNotFound);

        document.MarkProcessing();
        await _repository.UpdateAsync(document, cancellationToken);

        try
        {
            var embedding = await _embeddingService.GenerateEmbeddingAsync(
                document.Content, cancellationToken);

            await _embeddingService.StoreEmbeddingAsync(
                document.Id, embedding, cancellationToken);

            document.MarkIndexed();
        }
        catch (Exception ex)
        {
            document.MarkFailed(ex.Message);
        }

        await _repository.UpdateAsync(document, cancellationToken);

        return Result.Success();
    }
}
