using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Common.Events;

/// <summary>
/// Raised when a Corrective and Preventive Action (CAPA) is formally initiated.
/// A CAPA may originate from one or more non-conformances, audit findings,
/// or customer complaints. Consumed by the Notification module (to alert the assigned owner)
/// and the Training module (to evaluate whether retraining is required).
/// </summary>
public sealed record CAPAInitiatedEvent(
    Guid CapaId,
    Guid? SourceNonConformanceId,
    Guid? SourceAuditFindingId,
    Guid AssignedOwnerId) : DomainEvent;
