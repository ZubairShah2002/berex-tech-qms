namespace BerexQms.Application.AuditManagement.DTOs;

public sealed record AuditChecklistDto(
    Guid Id,
    Guid AuditRecordId,
    string Standard,
    string ClauseReference,
    string Requirement,
    bool IsCompliant,
    string? Evidence,
    string? Notes);
