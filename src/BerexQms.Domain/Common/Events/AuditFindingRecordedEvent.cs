using BerexQms.Domain.Common.Enums;
using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Common.Events;

/// <summary>
/// Raised when an audit finding is formally recorded during or after an audit.
/// Consumed by CAPA (to initiate corrective actions for major/critical findings),
/// Non-Conformance (to create NC records when the finding identifies a product defect),
/// and the AI Engine (for cross-audit trend analysis).
/// </summary>
public sealed record AuditFindingRecordedEvent(
    Guid FindingId,
    Guid AuditId,
    Severity Severity,
    string Area) : DomainEvent;
