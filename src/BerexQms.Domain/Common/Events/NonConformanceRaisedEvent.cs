using BerexQms.Domain.Common.Enums;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Common.Events;

/// <summary>
/// Raised when a new non-conformance record is created, whether from a failed inspection,
/// a line find, an audit observation, or a customer complaint.
/// Consumed by CAPA (to evaluate whether corrective action is warranted),
/// Supplier Quality (to update supplier scorecards and trigger SCARs),
/// and the AI Engine (for defect pattern analysis and repeat-defect detection).
/// </summary>
public sealed record NonConformanceRaisedEvent(
    Guid NonConformanceId,
    Severity Severity,
    Guid ProductId,
    string DefectType) : DomainEvent;
