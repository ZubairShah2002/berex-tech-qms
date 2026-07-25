using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Domain.ProductCatalog.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.ProductCatalog.Commands.ReleaseRevision;

public sealed class ReleaseRevisionCommandHandler : ICommandHandler<ReleaseRevisionCommand>
{
    private readonly IPartRepository _partRepository;
    private readonly ICurrentUserService _currentUserService;

    public ReleaseRevisionCommandHandler(IPartRepository partRepository, ICurrentUserService currentUserService)
    {
        _partRepository = partRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(ReleaseRevisionCommand request, CancellationToken cancellationToken)
    {
        var part = await _partRepository.GetWithRevisionsAsync(request.PartId, cancellationToken);
        if (part is null)
            return Result.Failure(PartErrors.NotFound);

        part.ReleaseRevision(request.RevisionId, _currentUserService.UserId.ToString());
        await _partRepository.UpdateAsync(part, cancellationToken);

        return Result.Success();
    }
}
