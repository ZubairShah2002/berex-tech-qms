using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.ProductCatalog.DTOs;
using BerexQms.Domain.ProductCatalog.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.ProductCatalog.Commands.AddBomReference;

public sealed class AddBomReferenceCommandHandler : ICommandHandler<AddBomReferenceCommand, BomReferenceDto>
{
    private readonly IPartRepository _partRepository;

    public AddBomReferenceCommandHandler(IPartRepository partRepository)
    {
        _partRepository = partRepository;
    }

    public async Task<Result<BomReferenceDto>> Handle(AddBomReferenceCommand request, CancellationToken cancellationToken)
    {
        var part = await _partRepository.GetWithBomReferencesAsync(request.PartId, cancellationToken);
        if (part is null)
            return PartErrors.NotFound;

        var childPart = await _partRepository.GetByIdAsync(request.ChildPartId, cancellationToken);
        if (childPart is null)
            return PartErrors.ChildPartNotFound;

        var bomRef = part.AddBomReference(request.ChildPartId, request.Quantity, request.ReferenceDesignator);
        await _partRepository.UpdateAsync(part, cancellationToken);

        return new BomReferenceDto(
            bomRef.Id,
            bomRef.ChildPartId,
            childPart.PartNumber,
            childPart.Name,
            bomRef.Quantity,
            bomRef.ReferenceDesignator,
            bomRef.SortOrder);
    }
}
