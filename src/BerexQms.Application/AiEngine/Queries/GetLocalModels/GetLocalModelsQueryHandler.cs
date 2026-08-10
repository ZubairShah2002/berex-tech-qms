using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetLocalModels;

internal sealed class GetLocalModelsQueryHandler
    : IQueryHandler<GetLocalModelsQuery, IReadOnlyList<AiLocalModelDto>>
{
    private readonly IAiOrchestrator _orchestrator;

    public GetLocalModelsQueryHandler(IAiOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task<Result<IReadOnlyList<AiLocalModelDto>>> Handle(
        GetLocalModelsQuery request, CancellationToken cancellationToken)
    {
        var models = await _orchestrator.GetLocalModelsAsync(cancellationToken);
        return Result<IReadOnlyList<AiLocalModelDto>>.Success(models);
    }
}
