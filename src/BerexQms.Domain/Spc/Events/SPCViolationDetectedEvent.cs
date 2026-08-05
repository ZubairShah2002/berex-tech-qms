using BerexQms.SharedKernel.Abstractions;

namespace BerexQms.Domain.Spc.Events;

public sealed record SPCViolationDetectedEvent(
    Guid ControlChartId,
    Guid DataPointId,
    string RuleViolation,
    decimal Value,
    decimal UpperControlLimit,
    decimal LowerControlLimit) : DomainEvent;
