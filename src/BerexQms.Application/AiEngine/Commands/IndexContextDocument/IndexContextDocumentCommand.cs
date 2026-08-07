using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.AiEngine.Commands.IndexContextDocument;

public sealed record IndexContextDocumentCommand(Guid DocumentId) : ICommand;
