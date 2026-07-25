using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.ProductCatalog.DTOs;
using BerexQms.Domain.ProductCatalog.Enums;
using BerexQms.Domain.ProductCatalog.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.ProductCatalog.Commands.AddSpecificationParameter;

public sealed class AddSpecificationParameterCommandHandler
    : ICommandHandler<AddSpecificationParameterCommand, SpecificationParameterDto>
{
    private readonly IPartRepository _partRepository;

    public AddSpecificationParameterCommandHandler(IPartRepository partRepository)
    {
        _partRepository = partRepository;
    }

    public async Task<Result<SpecificationParameterDto>> Handle(
        AddSpecificationParameterCommand request, CancellationToken cancellationToken)
    {
        var part = await _partRepository.GetWithRevisionsAsync(request.PartId, cancellationToken);
        if (part is null)
            return PartErrors.NotFound;

        if (!part.Revisions.Any(r => r.Id == request.RevisionId))
            return PartErrors.RevisionNotFound;

        if (!Enum.TryParse<ParameterType>(request.Type, true, out var paramType))
            paramType = ParameterType.Other;

        var param = part.AddSpecificationParameter(
            request.RevisionId,
            request.Name, paramType, request.Unit,
            request.NominalValue, request.UpperTolerance, request.LowerTolerance,
            request.TextValue, request.IsCritical);

        await _partRepository.UpdateAsync(part, cancellationToken);

        return new SpecificationParameterDto(
            param.Id, param.Name, param.Type.ToString(), param.Unit,
            param.NominalValue, param.UpperTolerance, param.LowerTolerance,
            param.TextValue, param.IsCritical, param.SortOrder);
    }
}
