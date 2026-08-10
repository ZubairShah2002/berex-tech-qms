using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetAiProviderStatus;

internal sealed class GetAiProviderStatusQueryHandler
    : IQueryHandler<GetAiProviderStatusQuery, IReadOnlyList<AiProviderStatusDto>>
{
    private readonly IAiOrchestrator _orchestrator;

    public GetAiProviderStatusQueryHandler(IAiOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task<Result<IReadOnlyList<AiProviderStatusDto>>> Handle(
        GetAiProviderStatusQuery request, CancellationToken cancellationToken)
    {
        var status = await _orchestrator.GetProviderStatusAsync(cancellationToken);
        return Result<IReadOnlyList<AiProviderStatusDto>>.Success(status);
    }
}
