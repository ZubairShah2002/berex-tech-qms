using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Events;

public sealed record AiContextDocumentIndexedEvent(
    Guid ContextDocumentId,
    string ContextType,
    string SourceModule) : DomainEvent;
