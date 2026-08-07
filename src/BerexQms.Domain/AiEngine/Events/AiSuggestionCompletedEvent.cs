using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.AiEngine.Events;

/// <summary>
/// Raised when an AI prediction/suggestion completes successfully, allowing
/// downstream consumers (e.g. analytics ingestion, notifications) to react.
/// </summary>
public sealed record AiSuggestionCompletedEvent(
    Guid InteractionId,
    string Capability,
    decimal ConfidenceScore,
    Guid UserId) : DomainEvent;
