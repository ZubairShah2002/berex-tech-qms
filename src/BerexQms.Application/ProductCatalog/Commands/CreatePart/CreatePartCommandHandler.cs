using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.Interfaces;
using BerexQms.Application.ProductCatalog.DTOs;
using BerexQms.Domain.ProductCatalog.Entities;
using BerexQms.Domain.ProductCatalog.Enums;
using BerexQms.Domain.ProductCatalog.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.ProductCatalog.Commands.CreatePart;

public sealed class CreatePartCommandHandler : ICommandHandler<CreatePartCommand, PartDto>
{
    private readonly IPartRepository _partRepository;
    private readonly ITenantContext _tenantContext;

    public CreatePartCommandHandler(IPartRepository partRepository, ITenantContext tenantContext)
    {
        _partRepository = partRepository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<PartDto>> Handle(CreatePartCommand request, CancellationToken cancellationToken)
    {
        if (await _partRepository.PartNumberExistsAsync(request.PartNumber, cancellationToken))
            return PartErrors.PartNumberExists;

        if (!Enum.TryParse<SerializationMode>(request.SerializationMode, true, out var serializationMode))
            serializationMode = SerializationMode.None;

        var part = Part.Create(
            Guid.NewGuid(),
            _tenantContext.CurrentTenantId,
            request.PartNumber,
            request.Name,
            request.Description,
            request.ProductFamily,
            request.Category,
            serializationMode,
            request.UnitOfMeasure);

        await _partRepository.AddAsync(part, cancellationToken);

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
            null,
            0,
            part.CreatedAt);
    }
}
