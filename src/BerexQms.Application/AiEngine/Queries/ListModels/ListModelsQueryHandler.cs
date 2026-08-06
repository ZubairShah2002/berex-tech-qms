using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Specifications;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.ListModels;

internal sealed class ListModelsQueryHandler : IQueryHandler<ListModelsQuery, PagedResult<AiModelDto>>
{
    private readonly IAiModelRepository _repository;

    public ListModelsQueryHandler(IAiModelRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<AiModelDto>>> Handle(
        ListModelsQuery request, CancellationToken cancellationToken)
    {
        var spec = new AiModelFilterSpec(request.Capability, request.Status, request.Page, request.PageSize);

        var models = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);

        var dtos = models.Select(MapToDto).ToList();

        return new PagedResult<AiModelDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    internal static AiModelDto MapToDto(AiModel model)
    {
        return new AiModelDto(
            model.Id,
            model.Name,
            model.Version,
            model.Capability,
            model.Status,
            model.Description,
            model.TrainingSampleCount,
            model.TrainedAt,
            model.PromotedAt,
            model.CreatedAt);
    }
}
