using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Common.Events;

/// <summary>
/// Raised when an inspection has been fully completed and its result recorded.
/// Consumed by the Non-Conformance module (to auto-create NCs on failure),
/// the Supplier Quality module (to update supplier scorecards for IQC inspections),
/// and the AI Engine (for trend analysis and defect prediction).
/// </summary>
public sealed record InspectionCompletedEvent(
    Guid InspectionId,
    string Result,
    Guid ProductId,
    DateTime CompletedAt) : DomainEvent;
