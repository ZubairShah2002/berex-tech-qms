using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;
using BerexQms.Domain.AiEngine.Repositories;
using BerexQms.SharedKernel.Results;

namespace BerexQms.Application.AiEngine.Queries.GetInteractionById;

internal sealed class GetInteractionByIdQueryHandler
    : IQueryHandler<GetInteractionByIdQuery, AiInteractionDetailDto>
{
    private readonly IAiInteractionRepository _repository;

    public GetInteractionByIdQueryHandler(IAiInteractionRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AiInteractionDetailDto>> Handle(
        GetInteractionByIdQuery request, CancellationToken cancellationToken)
    {
        var interaction = await _repository.GetByIdAsync(request.InteractionId, cancellationToken);
        if (interaction is null)
            return AiEngineErrors.InteractionNotFound;

        var sourceReferences = AiSourceReferenceSerializer.Deserialize(interaction.SourceReferences);

        return new AiInteractionDetailDto(
            interaction.Id,
            interaction.Capability,
            interaction.UserId,
            interaction.ModelId,
            interaction.OutputSummary,
            interaction.Confidence?.Score,
            interaction.Confidence?.Level.ToString(),
            sourceReferences,
            interaction.Status,
            interaction.UserAction,
            interaction.UserJustification,
            interaction.RequestedAt,
            interaction.CompletedAt,
            interaction.ResponseTimeMs,
            interaction.InputSummary,
            interaction.CreatedBy,
            interaction.CreatedAt);
    }
}
