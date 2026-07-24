using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Common.Events;

/// <summary>
/// Raised when a user's qualification (e.g., inspector certification, auditor accreditation)
/// is approaching its expiry date. Consumed by the Notification module (to alert the user
/// and their supervisor) and the Inspection module (to flag integrity gate warnings
/// for inspections requiring the expiring qualification).
/// </summary>
public sealed record QualificationExpiringEvent(
    Guid QualificationUserId,
    Guid QualificationId,
    DateTime ExpiryDate) : DomainEvent;
