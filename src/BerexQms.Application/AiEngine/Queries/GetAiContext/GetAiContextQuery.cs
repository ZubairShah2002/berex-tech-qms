using BerexQms.Application.Abstractions.Messaging;
using BerexQms.Application.AiEngine.DTOs;

namespace BerexQms.Application.AiEngine.Queries.GetAiContext;

public sealed record GetAiContextQuery(
    string SourceModule,
    string? ContextType) : IQuery<IReadOnlyList<ContextDocumentDto>>;
