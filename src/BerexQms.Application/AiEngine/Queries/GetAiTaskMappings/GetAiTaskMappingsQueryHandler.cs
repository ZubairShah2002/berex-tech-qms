using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetAiTaskMappings;

internal sealed class GetAiTaskMappingsQueryHandler
    : IQueryHandler<GetAiTaskMappingsQuery, IReadOnlyList<AiTaskMappingDto>>
{
    private readonly IAiOrchestrator _orchestrator;

    public GetAiTaskMappingsQueryHandler(IAiOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public Task<Result<IReadOnlyList<AiTaskMappingDto>>> Handle(
        GetAiTaskMappingsQuery request, CancellationToken cancellationToken)
    {
        var mappings = _orchestrator.GetTaskMappings();
        return Task.FromResult(Result<IReadOnlyList<AiTaskMappingDto>>.Success(mappings));
    }
}
