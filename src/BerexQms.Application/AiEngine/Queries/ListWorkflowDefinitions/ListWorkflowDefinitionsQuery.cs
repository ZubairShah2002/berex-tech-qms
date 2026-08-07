using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.ListWorkflowDefinitions;

public sealed record ListWorkflowDefinitionsQuery(bool ActiveOnly = true)
    : IQuery<IReadOnlyList<AiWorkflowDefinitionDto>>;
