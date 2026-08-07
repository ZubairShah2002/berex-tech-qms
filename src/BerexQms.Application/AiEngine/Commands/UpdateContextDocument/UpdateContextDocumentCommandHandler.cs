using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.UpdateContextDocument;

internal sealed class UpdateContextDocumentCommandHandler
    : ICommandHandler<UpdateContextDocumentCommand>
{
    private readonly IAiContextDocumentRepository _repository;

    public UpdateContextDocumentCommandHandler(IAiContextDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(
        UpdateContextDocumentCommand request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null)
            return Result.Failure(AiEngineErrors.ContextDocumentNotFound);

        document.UpdateContent(request.Title, request.Content, request.MetadataJson);

        await _repository.UpdateAsync(document, cancellationToken);

        return Result.Success();
    }
}
