namespace BerexQms.Application.AuditManagement.DTOs;

public sealed record AuditPlanDto(
    Guid Id,
    string PlanName,
    int Year,
    string? Description,
    string? Scope,
    bool IsActive,
    int AuditCount,
    DateTime CreatedAt);
