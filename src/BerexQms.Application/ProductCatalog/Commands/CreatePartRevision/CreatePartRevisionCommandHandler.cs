using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.ProductCatalog.DTOs;
using BerexQms.Domain.ProductCatalog.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.ProductCatalog.Commands.CreatePartRevision;

public sealed class CreatePartRevisionCommandHandler : ICommandHandler<CreatePartRevisionCommand, PartRevisionDto>
{
    private readonly IPartRepository _partRepository;

    public CreatePartRevisionCommandHandler(IPartRepository partRepository)
    {
        _partRepository = partRepository;
    }

    public async Task<Result<PartRevisionDto>> Handle(CreatePartRevisionCommand request, CancellationToken cancellationToken)
    {
        var part = await _partRepository.GetWithRevisionsAsync(request.PartId, cancellationToken);
        if (part is null)
            return PartErrors.NotFound;

        var revision = part.CreateRevision(request.RevisionCode, request.Description, request.ChangeReason);
        await _partRepository.UpdateAsync(part, cancellationToken);

        return new PartRevisionDto(
            revision.Id,
            revision.RevisionCode,
            revision.Status.ToString(),
            revision.Description,
            revision.ChangeReason,
            revision.ReleasedAt,
            revision.ReleasedBy,
            revision.ObsoletedAt,
            [],
            revision.CreatedAt);
    }
}
