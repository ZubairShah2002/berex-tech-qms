using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.ListKnowledgeSources;

internal sealed class ListKnowledgeSourcesQueryHandler
    : IQueryHandler<ListKnowledgeSourcesQuery, IReadOnlyList<KnowledgeSourceDto>>
{
    private readonly IAiKnowledgeSourceRepository _repository;

    public ListKnowledgeSourcesQueryHandler(IAiKnowledgeSourceRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<KnowledgeSourceDto>>> Handle(
        ListKnowledgeSourcesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<AiKnowledgeSource> sources;

        if (request.ActiveOnly == true)
        {
            sources = await _repository.GetActiveSourcesAsync(cancellationToken);
        }
        else
        {
            sources = await _repository.ListAllAsync(cancellationToken);
        }

        var dtos = sources.Select(MapToDto).ToList();

        return dtos;
    }

    internal static KnowledgeSourceDto MapToDto(AiKnowledgeSource source)
    {
        return new KnowledgeSourceDto(
            source.Id,
            source.Name,
            source.Module,
            source.Description,
            source.IsActive,
            source.LastSyncedAt,
            source.DocumentCount,
            source.CreatedAt);
    }
}
