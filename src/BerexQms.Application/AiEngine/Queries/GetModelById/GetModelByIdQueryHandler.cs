using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetModelById;

internal sealed class GetModelByIdQueryHandler : IQueryHandler<GetModelByIdQuery, AiModelDetailDto>
{
    private readonly IAiModelRepository _repository;

    public GetModelByIdQueryHandler(IAiModelRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AiModelDetailDto>> Handle(
        GetModelByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await _repository.GetByIdAsync(request.ModelId, cancellationToken);
        if (model is null)
            return AiEngineErrors.ModelNotFound;

        return new AiModelDetailDto(
            model.Id,
            model.Name,
            model.Version,
            model.Capability,
            model.Status,
            model.Description,
            model.TrainingSampleCount,
            model.TrainedAt,
            model.PromotedAt,
            model.CreatedAt,
            model.TrainingMetrics,
            model.ValidationMetrics,
            model.HyperParameters,
            model.DataSnapshotReference,
            model.RetiredAt,
            model.CreatedBy);
    }
}
