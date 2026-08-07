using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.ListKnowledgeSources;

public sealed record ListKnowledgeSourcesQuery(bool? ActiveOnly) : IQuery<IReadOnlyList<KnowledgeSourceDto>>;
