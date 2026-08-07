using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.ListCapabilityConfigs;

public sealed record ListCapabilityConfigsQuery : IQuery<IReadOnlyList<AiCapabilityConfigDto>>;
