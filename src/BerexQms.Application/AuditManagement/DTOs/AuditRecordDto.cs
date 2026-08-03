namespace BerexQms.Application.AuditManagement.DTOs;

public sealed record AuditRecordDto(
    Guid Id,
    string AuditNumber,
    string AuditType,
    string Status,
    string LeadAuditorId,
    string? AuditeeArea,
    DateTime ScheduledDate,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    int FindingCount,
    bool HasReport);
