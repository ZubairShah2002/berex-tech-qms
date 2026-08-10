using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Interfaces;
using BerexQms.Application.Interfaces;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Commands.ExecuteAiAnalysis;

internal sealed class ExecuteAiAnalysisCommandHandler
    : ICommandHandler<ExecuteAiAnalysisCommand, AiProviderResponseDto>
{
    private readonly IAiOrchestrator _orchestrator;
    private readonly ICurrentUserService _currentUser;

    public ExecuteAiAnalysisCommandHandler(
        IAiOrchestrator orchestrator,
        ICurrentUserService currentUser)
    {
        _orchestrator = orchestrator;
        _currentUser = currentUser;
    }

    public async Task<Result<AiProviderResponseDto>> Handle(
        ExecuteAiAnalysisCommand request, CancellationToken cancellationToken)
    {
        var orchestratorRequest = new AiOrchestratorRequest
        {
            TaskType = request.TaskType,
            Module = request.Module,
            Content = request.Content,
            EntityId = request.EntityId,
            MaxContextDocuments = request.MaxContextDocuments,
            UserId = _currentUser.IsAuthenticated ? _currentUser.UserId : null,
        };

        var response = await _orchestrator.ExecuteAsync(orchestratorRequest, cancellationToken);

        if (!response.Success)
            return AiEngineErrors.AiProviderRequestFailed;

        return response;
    }
}
