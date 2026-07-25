using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.ProductCatalog.DTOs;
using BerexQms.Domain.ProductCatalog.Enums;
using BerexQms.Domain.ProductCatalog.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.ProductCatalog.Commands.UpdatePart;

public sealed class UpdatePartCommandHandler : ICommandHandler<UpdatePartCommand, PartDto>
{
    private readonly IPartRepository _partRepository;

    public UpdatePartCommandHandler(IPartRepository partRepository)
    {
        _partRepository = partRepository;
    }

    public async Task<Result<PartDto>> Handle(UpdatePartCommand request, CancellationToken cancellationToken)
    {
        var part = await _partRepository.GetWithRevisionsAsync(request.PartId, cancellationToken);
        if (part is null)
            return PartErrors.NotFound;

        if (!Enum.TryParse<SerializationMode>(request.SerializationMode, true, out var serializationMode))
            serializationMode = SerializationMode.None;

        part.UpdateDetails(
            request.Name,
            request.Description,
            request.ProductFamily,
            request.Category,
            serializationMode,
            request.UnitOfMeasure);

        await _partRepository.UpdateAsync(part, cancellationToken);

        var currentRevision = part.GetCurrentRevision();

        return new PartDto(
            part.Id,
            part.PartNumber,
            part.Name,
            part.Description,
            part.ProductFamily,
            part.Category,
            part.SerializationMode.ToString(),
            part.Status.ToString(),
            part.UnitOfMeasure,
            currentRevision?.RevisionCode,
            part.Revisions.Count,
            part.CreatedAt);
    }
}
