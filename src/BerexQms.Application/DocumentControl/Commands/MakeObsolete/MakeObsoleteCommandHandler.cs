using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.DocumentControl.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.DocumentControl.Commands.MakeObsolete;

internal sealed class MakeObsoleteCommandHandler : ICommandHandler<MakeObsoleteCommand>
{
    private readonly IDocumentRepository _repository;

    public MakeObsoleteCommandHandler(IDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(MakeObsoleteCommand request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetWithVersionsAsync(request.DocumentId, cancellationToken);
        if (document is null)
            return Result.Failure(DocumentErrors.NotFound);

        document.MakeObsolete();
        return Result.Success();
    }
}
