using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Common.Events;

/// <summary>
/// Raised when a supplier's quality score is recalculated after new inspection results,
/// non-conformances, or SCAR closures are recorded.
/// Consumed by the Notification module (to alert procurement when a supplier
/// falls below acceptable thresholds) and dashboards for supplier risk monitoring.
/// </summary>
public sealed record SupplierScoreUpdatedEvent(
    Guid SupplierId,
    decimal NewScore,
    string EvaluationPeriod) : DomainEvent;
