using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.GetCapabilityStats;

public sealed record GetCapabilityStatsQuery(string Capability, int Days) : IQuery<AiCapabilityStatsDto>;
