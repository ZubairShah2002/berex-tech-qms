using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.CreateContextDocument;

public sealed record CreateContextDocumentCommand(
    string SourceModule,
    string? SourceEntityId,
    string ContextType,
    string Title,
    string Content,
    string? MetadataJson) : ICommand<Guid>;
