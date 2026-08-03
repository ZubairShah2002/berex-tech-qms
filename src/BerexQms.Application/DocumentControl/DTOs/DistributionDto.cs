namespace BerexQms.Application.DocumentControl.DTOs;

public sealed record DistributionDto(
    Guid Id,
    string RecipientId,
    DateTime DistributedAt,
    DateTime? AcknowledgedAt,
    DateTime ComplianceDeadline,
    bool IsOverdue);
