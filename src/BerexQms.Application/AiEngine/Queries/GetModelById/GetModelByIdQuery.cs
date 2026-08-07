using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.GetModelById;

public sealed record GetModelByIdQuery(Guid ModelId) : IQuery<AiModelDetailDto>;
