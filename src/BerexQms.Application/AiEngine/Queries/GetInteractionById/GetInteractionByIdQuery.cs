using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.GetInteractionById;

public sealed record GetInteractionByIdQuery(Guid InteractionId) : IQuery<AiInteractionDetailDto>;
