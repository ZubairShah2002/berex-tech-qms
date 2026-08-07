using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.GetContextStats;

public sealed record GetContextStatsQuery() : IQuery<ContextStatsDto>;
