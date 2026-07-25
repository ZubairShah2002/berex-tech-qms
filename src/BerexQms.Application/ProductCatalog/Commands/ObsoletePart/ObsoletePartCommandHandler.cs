using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Domain.ProductCatalog.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.ProductCatalog.Commands.ObsoletePart;

public sealed class ObsoletePartCommandHandler : ICommandHandler<ObsoletePartCommand>
{
    private readonly IPartRepository _partRepository;

    public ObsoletePartCommandHandler(IPartRepository partRepository)
    {
        _partRepository = partRepository;
    }

    public async Task<Result> Handle(ObsoletePartCommand request, CancellationToken cancellationToken)
    {
        var part = await _partRepository.GetWithRevisionsAsync(request.PartId, cancellationToken);
        if (part is null)
            return Result.Failure(PartErrors.NotFound);

        part.Obsolete();
        await _partRepository.UpdateAsync(part, cancellationToken);

        return Result.Success();
    }
}
