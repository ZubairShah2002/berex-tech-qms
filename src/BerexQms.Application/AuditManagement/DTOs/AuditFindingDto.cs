namespace BerexQms.Application.AuditManagement.DTOs;

public sealed record AuditFindingDto(
    Guid Id,
    Guid AuditRecordId,
    string Classification,
    string ClauseReference,
    string Description,
    string? Evidence,
    string? CorrectiveAction,
    string? LinkedCapaId,
    DateTime FoundAt);
