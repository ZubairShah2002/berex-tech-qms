using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.UpdateContextDocument;

public sealed record UpdateContextDocumentCommand(
    Guid DocumentId,
    string Title,
    string Content,
    string? MetadataJson) : ICommand;
