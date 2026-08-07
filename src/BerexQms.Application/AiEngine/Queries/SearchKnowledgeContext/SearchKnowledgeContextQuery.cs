using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.SearchKnowledgeContext;

public sealed record SearchKnowledgeContextQuery(
    string SearchTerm,
    string? SourceModule,
    int MaxResults) : IQuery<IReadOnlyList<ContextSearchResultDto>>;
