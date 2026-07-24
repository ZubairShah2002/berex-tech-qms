using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Common.Events;

/// <summary>
/// Raised when a controlled document is formally approved and becomes effective.
/// Consumed by the Training module (to trigger re-qualification when procedures change)
/// and the Audit module (to update the effective document register for audit reference).
/// </summary>
public sealed record DocumentApprovedEvent(
    Guid DocumentId,
    int Version,
    Guid ApproverId,
    DateTime EffectiveDate) : DomainEvent;
