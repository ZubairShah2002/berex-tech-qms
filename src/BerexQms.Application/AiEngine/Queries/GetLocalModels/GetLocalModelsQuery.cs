using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.GetLocalModels;

public sealed record GetLocalModelsQuery() : IQuery<IReadOnlyList<AiLocalModelDto>>;
