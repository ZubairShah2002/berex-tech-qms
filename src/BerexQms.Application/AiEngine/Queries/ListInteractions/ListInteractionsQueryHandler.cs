using BerexQms.Application.Abstractions;
using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Application.AiEngine.Specifications;
using BerexQms.Domain.AiEngine.Entities;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.ListInteractions;

internal sealed class ListInteractionsQueryHandler
    : IQueryHandler<ListInteractionsQuery, PagedResult<AiInteractionDto>>
{
    private readonly IAiInteractionRepository _repository;

    public ListInteractionsQueryHandler(IAiInteractionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PagedResult<AiInteractionDto>>> Handle(
        ListInteractionsQuery request, CancellationToken cancellationToken)
    {
        var spec = new AiInteractionFilterSpec(
            request.Capability, request.Status, request.UserAction, request.Page, request.PageSize);

        var interactions = await _repository.ListAsync(spec, cancellationToken);
        var totalCount = await _repository.CountAsync(spec, cancellationToken);

        var dtos = interactions.Select(MapToDto).ToList();

        return new PagedResult<AiInteractionDto>(dtos, totalCount, request.Page, request.PageSize);
    }

    internal static AiInteractionDto MapToDto(AiInteraction interaction)
    {
        return new AiInteractionDto(
            interaction.Id,
            interaction.Capability,
            interaction.UserId,
            interaction.ModelId,
            interaction.OutputSummary,
            interaction.Confidence?.Score,
            interaction.Confidence?.Level.ToString(),
            AiSourceReferenceSerializer.Deserialize(interaction.SourceReferences),
            interaction.Status,
            interaction.UserAction,
            interaction.UserJustification,
            interaction.RequestedAt,
            interaction.CompletedAt,
            interaction.ResponseTimeMs);
    }
}
