using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Common.Events;

/// <summary>
/// Raised when a user completes a training course and achieves a competency level.
/// Consumed by the Inspection module (to update inspector qualification status)
/// and the Notification module (to inform supervisors of newly qualified personnel).
/// </summary>
public sealed record TrainingCompletedEvent(
    Guid TrainingUserId,
    Guid CourseId,
    string CompetencyLevel,
    DateTime? ExpiryDate) : DomainEvent;
