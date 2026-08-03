namespace BerexQms.Application.AuditManagement.DTOs;

public sealed record AuditPlanDetailDto(
    Guid Id,
    string PlanName,
    int Year,
    string? Description,
    string? Scope,
    bool IsActive,
    IReadOnlyList<AuditRecordDto> Audits,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? ModifiedAt);
