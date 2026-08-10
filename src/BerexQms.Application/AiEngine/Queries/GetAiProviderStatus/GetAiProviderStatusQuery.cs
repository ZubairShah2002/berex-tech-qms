using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.GetAiProviderStatus;

public sealed record GetAiProviderStatusQuery() : IQuery<IReadOnlyList<AiProviderStatusDto>>;
