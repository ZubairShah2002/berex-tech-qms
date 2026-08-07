using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.ListWorkflowDefinitions;

internal sealed class ListWorkflowDefinitionsQueryHandler
    : IQueryHandler<ListWorkflowDefinitionsQuery, IReadOnlyList<AiWorkflowDefinitionDto>>
{
    private readonly IAiWorkflowDefinitionRepository _repository;

    public ListWorkflowDefinitionsQueryHandler(IAiWorkflowDefinitionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<AiWorkflowDefinitionDto>>> Handle(
        ListWorkflowDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var definitions = request.ActiveOnly
            ? await _repository.GetActiveWorkflowsAsync(cancellationToken)
            : await _repository.ListAllAsync(cancellationToken);

        var dtos = definitions.Select(d => new AiWorkflowDefinitionDto(
            d.Id,
            d.Name,
            d.Description,
            d.MinimumPermissionLevel,
            d.Category,
            d.IsActive,
            d.AffectedModules,
            d.CreatedAt)).ToList();

        return Result.Success<IReadOnlyList<AiWorkflowDefinitionDto>>(dtos);
    }
}
