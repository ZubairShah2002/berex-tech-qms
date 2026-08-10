using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.GetAiTaskMappings;

public sealed record GetAiTaskMappingsQuery() : IQuery<IReadOnlyList<AiTaskMappingDto>>;
