using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.ProductCatalog.DTOs;
using BerexQms.Domain.ProductCatalog.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.ProductCatalog.Queries.GetPartById;

public sealed class GetPartByIdQueryHandler : IQueryHandler<GetPartByIdQuery, PartDetailDto>
{
    private readonly IPartRepository _partRepository;

    public GetPartByIdQueryHandler(IPartRepository partRepository)
    {
        _partRepository = partRepository;
    }

    public async Task<Result<PartDetailDto>> Handle(GetPartByIdQuery request, CancellationToken cancellationToken)
    {
        var part = await _partRepository.GetWithRevisionsAsync(request.PartId, cancellationToken);
        if (part is null)
            return PartErrors.NotFound;

        var partWithBom = await _partRepository.GetWithBomReferencesAsync(request.PartId, cancellationToken);

        var revisionDtos = part.Revisions
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new PartRevisionDto(
                r.Id,
                r.RevisionCode,
                r.Status.ToString(),
                r.Description,
                r.ChangeReason,
                r.ReleasedAt,
                r.ReleasedBy,
                r.ObsoletedAt,
                r.SpecificationParameters
                    .OrderBy(sp => sp.SortOrder)
                    .Select(sp => new SpecificationParameterDto(
                        sp.Id, sp.Name, sp.Type.ToString(), sp.Unit,
                        sp.NominalValue, sp.UpperTolerance, sp.LowerTolerance,
                        sp.TextValue, sp.IsCritical, sp.SortOrder))
                    .ToList(),
                r.CreatedAt))
            .ToList();

        var bomDtos = new List<BomReferenceDto>();
        if (partWithBom is not null)
        {
            foreach (var bom in partWithBom.BomReferences.OrderBy(b => b.SortOrder))
            {
                var childPart = await _partRepository.GetByIdAsync(bom.ChildPartId, cancellationToken);
                bomDtos.Add(new BomReferenceDto(
                    bom.Id,
                    bom.ChildPartId,
                    childPart?.PartNumber ?? "Unknown",
                    childPart?.Name ?? "Unknown",
                    bom.Quantity,
                    bom.ReferenceDesignator,
                    bom.SortOrder));
            }
        }

        return new PartDetailDto(
            part.Id,
            part.PartNumber,
            part.Name,
            part.Description,
            part.ProductFamily,
            part.Category,
            part.SerializationMode.ToString(),
            part.Status.ToString(),
            part.UnitOfMeasure,
            revisionDtos,
            bomDtos,
            part.CreatedAt,
            part.CreatedBy,
            part.ModifiedAt);
    }
}
