using BerexQms.Application.Abstractions.Messaging;

namespace BerexQms.Application.DocumentControl.Commands.CreateDocument;

public sealed record CreateDocumentCommand(
    string DocumentNumber,
    string Title,
    string DocumentType,
    string? Description,
    string? Department) : ICommand<Guid>;
