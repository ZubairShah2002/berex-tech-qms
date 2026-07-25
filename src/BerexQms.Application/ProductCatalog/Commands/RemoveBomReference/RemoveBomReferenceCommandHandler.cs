using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.ProductCatalog.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.ProductCatalog.Commands.RemoveBomReference;

public sealed class RemoveBomReferenceCommandHandler : ICommandHandler<RemoveBomReferenceCommand>
{
    private readonly IPartRepository _partRepository;

    public RemoveBomReferenceCommandHandler(IPartRepository partRepository)
    {
        _partRepository = partRepository;
    }

    public async Task<Result> Handle(RemoveBomReferenceCommand request, CancellationToken cancellationToken)
    {
        var part = await _partRepository.GetWithBomReferencesAsync(request.PartId, cancellationToken);
        if (part is null)
            return Result.Failure(PartErrors.NotFound);

        part.RemoveBomReference(request.BomReferenceId);
        await _partRepository.UpdateAsync(part, cancellationToken);

        return Result.Success();
    }
}
